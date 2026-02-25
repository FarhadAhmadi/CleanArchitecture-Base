using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Database;
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Web.Api.Infrastructure;

namespace Web.Api.Middleware;

internal sealed class RequestIdempotencyMiddleware(
    RequestDelegate next,
    IdempotencyOptions options,
    ILogger<RequestIdempotencyMiddleware> logger)
{
    private const string IdempotencyHeaderName = "Idempotency-Key";

    public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
    {
        if (!options.Enabled || !ShouldHandle(context.Request.Method))
        {
            await next(context);
            return;
        }

        string idempotencyKey = context.Request.Headers[IdempotencyHeaderName].FirstOrDefault()?.Trim() ?? string.Empty;
        if (idempotencyKey.Length == 0)
        {
            await next(context);
            return;
        }

        if (idempotencyKey.Length > 120)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = "Idempotency-Key is too long." });
            return;
        }

        DateTime now = DateTime.UtcNow;
        string scope = BuildScope(context);
        string scopeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scope)));

        IdempotencyRequest? existing = await dbContext.IdempotencyRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ScopeHash == scopeHash && x.Key == idempotencyKey && x.ExpiresAtUtc >= now,
                context.RequestAborted);

        if (existing is not null)
        {
            await WriteExistingResponseAsync(context, existing);
            return;
        }

        IdempotencyRequest request = new()
        {
            Id = Guid.NewGuid(),
            Key = idempotencyKey,
            ScopeHash = scopeHash,
            Scope = scope.Length <= 200 ? scope : scope[..200],
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(Math.Max(1, options.ExpirationMinutes)),
            IsCompleted = false
        };

        dbContext.IdempotencyRequests.Add(request);

        try
        {
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (DbUpdateException)
        {
            bool raceWinnerHandled = await TryServeRaceWinnerAsync(context, dbContext, scopeHash, idempotencyKey, now);
            if (raceWinnerHandled)
            {
                return;
            }

            throw;
        }

        Stream originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await next(context);

            if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                dbContext.IdempotencyRequests.Remove(request);
                await dbContext.SaveChangesAsync(context.RequestAborted);
                return;
            }

            await PersistFinalResponseAsync(context, dbContext, request, responseBuffer);
        }
        catch
        {
            dbContext.IdempotencyRequests.Remove(request);
            await dbContext.SaveChangesAsync(context.RequestAborted);
            throw;
        }
        finally
        {
            responseBuffer.Position = 0;
            context.Response.Body = originalResponseBody;
            await responseBuffer.CopyToAsync(originalResponseBody, context.RequestAborted);
        }
    }

    private static bool ShouldHandle(string method)
    {
        return HttpMethods.IsPost(method) ||
               HttpMethods.IsPut(method) ||
               HttpMethods.IsPatch(method) ||
               HttpMethods.IsDelete(method);
    }

    private async Task PersistFinalResponseAsync(
        HttpContext context,
        ApplicationDbContext dbContext,
        IdempotencyRequest request,
        MemoryStream responseBuffer)
    {
        request.IsCompleted = true;
        request.CompletedAtUtc = DateTime.UtcNow;
        request.StatusCode = context.Response.StatusCode;
        request.ContentType = context.Response.ContentType;

        int maxBytes = Math.Max(1, options.MaxResponseBodyBytes);
        request.ResponseBody = responseBuffer.Length == 0 || responseBuffer.Length > maxBytes
            ? null
            : responseBuffer.ToArray();

        dbContext.IdempotencyRequests.Update(request);
        await dbContext.SaveChangesAsync(context.RequestAborted);
    }

    private async Task<bool> TryServeRaceWinnerAsync(
        HttpContext context,
        ApplicationDbContext dbContext,
        string scopeHash,
        string idempotencyKey,
        DateTime now)
    {
        dbContext.ChangeTracker.Clear();

        IdempotencyRequest? existing = await dbContext.IdempotencyRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ScopeHash == scopeHash && x.Key == idempotencyKey && x.ExpiresAtUtc >= now,
                context.RequestAborted);

        if (existing is null)
        {
            return false;
        }

        await WriteExistingResponseAsync(context, existing);
        return true;
    }

    private async Task WriteExistingResponseAsync(HttpContext context, IdempotencyRequest existing)
    {
        if (!existing.IsCompleted)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = "A request with the same Idempotency-Key is in progress." });
            return;
        }

        context.Response.StatusCode = existing.StatusCode ?? StatusCodes.Status200OK;
        if (!string.IsNullOrWhiteSpace(existing.ContentType))
        {
            context.Response.ContentType = existing.ContentType;
        }

        if (existing.ResponseBody is { Length: > 0 })
        {
            await context.Response.Body.WriteAsync(existing.ResponseBody, context.RequestAborted);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Idempotent response replayed. Scope={Scope} Key={Key} StatusCode={StatusCode}",
                existing.Scope,
                existing.Key,
                existing.StatusCode);
        }
    }

    private static string BuildScope(HttpContext context)
    {
        string actorId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                         $"anon:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        string query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value! : string.Empty;
        return $"{context.Request.Method}:{context.Request.Path}{query}:{actorId}";
    }
}
