using Application.Abstractions.Data;
using Domain.Authorization;
using Domain.Logging;
using Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Logging;

public static class LoggingEndpoints
{
    private static readonly string[] RequiredSchemaFields =
    [
        "eventId", "timestampUtc", "level", "message", "source.service",
        "source.module", "traceId", "requestId", "tenantId", "actor.type",
        "actor.id", "outcome"
    ];

    public static IEndpointRouteBuilder MapLoggingEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/logging/v1")
            .WithTags("Logging");

        group.MapPost("/events", IngestSingle).HasPermission(LoggingPermissions.EventsWrite);
        group.MapPost("/events/bulk", IngestBulk).HasPermission(LoggingPermissions.EventsWrite);
        group.MapGet("/events", GetEvents).HasPermission(LoggingPermissions.EventsRead);
        group.MapGet("/events/corrupted", GetCorruptedEvents).HasPermission(LoggingPermissions.EventsRead);
        group.MapGet("/events/{eventId:guid}", GetEventById).HasPermission(LoggingPermissions.EventsRead);
        group.MapDelete("/events/{eventId:guid}", DeleteEvent).HasPermission(LoggingPermissions.EventsDelete);
        group.MapGet("/schema", GetSchema).HasPermission(LoggingPermissions.EventsRead);
        group.MapPost("/validate", ValidateInput).HasPermission(LoggingPermissions.EventsWrite);
        group.MapPost("/transform", TransformInput).HasPermission(LoggingPermissions.EventsWrite);
        group.MapGet("/health", GetHealth).HasPermission(LoggingPermissions.EventsRead);
        group.MapPost("/alerts/rules", CreateRule).HasPermission(LoggingPermissions.AlertsManage);
        group.MapGet("/alerts/rules", GetRules).HasPermission(LoggingPermissions.AlertsManage);
        group.MapPut("/alerts/rules/{id:guid}", UpdateRule).HasPermission(LoggingPermissions.AlertsManage);
        group.MapDelete("/alerts/rules/{id:guid}", DeleteRule).HasPermission(LoggingPermissions.AlertsManage);
        group.MapGet("/access-control", GetAccessControl).HasPermission(LoggingPermissions.AccessManage);
        group.MapPost("/access-control/roles", CreateRole).HasPermission(LoggingPermissions.AccessManage);
        group.MapPost("/access-control/assign", AssignAccess).HasPermission(LoggingPermissions.AccessManage);

        return app;
    }

    private static async Task<IResult> IngestSingle(
        IngestLogRequest request,
        HttpContext httpContext,
        ILogIngestionService ingestionService,
        CancellationToken cancellationToken)
    {
        string? idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
        IngestResult result = await ingestionService.IngestAsync(request, idempotencyKey, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> IngestBulk(
        BulkIngestRequest request,
        HttpContext httpContext,
        ILogIngestionService ingestionService,
        LoggingOptions options,
        CancellationToken cancellationToken)
    {
        if (request.Events.Count == 0 || request.Events.Count > options.MaxBulkItems)
        {
            return CustomResults.Problem(Result.Failure(
                Error.Problem("LOG-4002", $"Bulk size must be between 1 and {options.MaxBulkItems}.")));
        }

        List<IngestResult> results = [];
        for (int i = 0; i < request.Events.Count; i++)
        {
            string? key = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(key))
            {
                key = $"{key}:{i}";
            }

            IngestResult result = await ingestionService.IngestAsync(request.Events[i], key, cancellationToken);
            results.Add(result);
        }

        return Results.Ok(results);
    }

    private static async Task<IResult> GetEvents(
        IApplicationReadDbContext readContext,
        ILogIntegrityService integrityService,
        LogLevelType? level,
        DateTime? from,
        DateTime? to,
        string? actorId,
        string? service,
        string? module,
        string? traceId,
        string? outcome,
        string? text,
        int page,
        int pageSize,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken)
    {
        int normalizedPage = page <= 0 ? 1 : page;
        int normalizedPageSize = pageSize is <= 0 or > 200 ? 50 : pageSize;

        IQueryable<LogEvent> query = readContext.LogEvents.Where(x => !x.IsDeleted);

        if (level.HasValue)
        {
            query = query.Where(x => x.Level == level.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.TimestampUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.TimestampUtc <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(actorId))
        {
            query = query.Where(x => x.ActorId == actorId);
        }

        if (!string.IsNullOrWhiteSpace(service))
        {
            query = query.Where(x => x.SourceService == service);
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.SourceModule == module);
        }

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            query = query.Where(x => x.TraceId == traceId);
        }

        if (!string.IsNullOrWhiteSpace(outcome))
        {
            query = query.Where(x => x.Outcome == outcome);
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            query = query.Where(x => x.Message.Contains(text));
        }

        query = ApplySorting(query, sortBy, sortOrder);

        int total = await query.CountAsync(cancellationToken);
        List<LogEvent> items = await query
            .ApplyPaging(normalizedPage, normalizedPageSize)
            .ToListAsync(cancellationToken);

        var resultItems = items
            .Select(item => item.ToView(integrityService.IsCorrupted(item)))
            .ToList();

        return Results.Ok(new
        {
            page = normalizedPage,
            pageSize = normalizedPageSize,
            total,
            items = resultItems
        });
    }

    private static async Task<IResult> GetCorruptedEvents(
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        ILogIntegrityService integrityService,
        bool recalculate,
        CancellationToken cancellationToken)
    {
        List<LogEvent> events = await readContext.LogEvents
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(5000)
            .ToListAsync(cancellationToken);

        List<LogEventView> corrupted = [];

        foreach (LogEvent logEvent in events)
        {
            bool mismatch = integrityService.IsCorrupted(logEvent) || logEvent.HasIntegrityIssue;
            if (!mismatch)
            {
                continue;
            }

            corrupted.Add(logEvent.ToView(true));

            if (recalculate && !logEvent.HasIntegrityIssue)
            {
                LogEvent? tracked = await writeContext.LogEvents
                    .SingleOrDefaultAsync(x => x.Id == logEvent.Id, cancellationToken);

                tracked?.HasIntegrityIssue = true;
            }
        }

        if (recalculate)
        {
            await writeContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(new { total = corrupted.Count, items = corrupted });
    }

    private static async Task<IResult> GetEventById(
        Guid eventId,
        IApplicationReadDbContext readContext,
        ILogIntegrityService integrityService,
        CancellationToken cancellationToken)
    {
        LogEvent? item = await readContext.LogEvents
            .SingleOrDefaultAsync(x => x.Id == eventId && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(item.ToView(integrityService.IsCorrupted(item)));
    }

    private static async Task<IResult> DeleteEvent(
        Guid eventId,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        LogEvent? item = await writeContext.LogEvents.SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        if (item is null)
        {
            return Results.NotFound();
        }

        item.IsDeleted = true;
        item.DeletedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static IResult GetSchema()
    {
        return Results.Ok(new { version = "1.0", required = RequiredSchemaFields });
    }

    private static IResult ValidateInput(IngestLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return CustomResults.Problem(Result.Failure(Error.Problem("LOG-4001", "Message is required.")));
        }

        if (string.IsNullOrWhiteSpace(request.SourceService) || string.IsNullOrWhiteSpace(request.SourceModule))
        {
            return CustomResults.Problem(Result.Failure(
                Error.Problem("LOG-4001", "SourceService and SourceModule are required.")));
        }

        return Results.Ok(new { valid = true });
    }

    private static IResult TransformInput(IngestLogRequest request, ILogSanitizer sanitizer)
    {
        IngestLogRequest transformed = sanitizer.Sanitize(request);
        return Results.Ok(transformed);
    }

    private static IResult GetHealth(LoggingHealthService healthService) =>
        Results.Ok(healthService.GetHealth());

    private static async Task<IResult> CreateRule(
        CreateAlertRuleRequest request,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        AlertRule entity = request.ToEntity();
        writeContext.AlertRules.Add(entity);
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(entity);
    }

    private static async Task<IResult> GetRules(
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        List<AlertRule> rules = await readContext.AlertRules.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Results.Ok(rules);
    }

    private static async Task<IResult> UpdateRule(
        Guid id,
        CreateAlertRuleRequest request,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        AlertRule? rule = await writeContext.AlertRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rule is null)
        {
            return Results.NotFound();
        }

        request.Update(rule);
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(rule);
    }

    private static async Task<IResult> DeleteRule(
        Guid id,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        AlertRule? rule = await writeContext.AlertRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rule is null)
        {
            return Results.NotFound();
        }

        writeContext.AlertRules.Remove(rule);
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAccessControl(
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        List<string> permissions = await readContext.Permissions
            .Where(x => x.Code.StartsWith("logging.", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var roles = await readContext.Roles
            .Select(role => new
            {
                role.Name,
                Permissions = (
                    from rp in readContext.RolePermissions
                    join p in readContext.Permissions on rp.PermissionId equals p.Id
                    where rp.RoleId == role.Id && p.Code.StartsWith("logging.")
                    select p.Code).ToList()
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(new { permissions, roles });
    }

    private static async Task<IResult> CreateRole(
        CreateRoleRequest request,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        bool exists = await writeContext.Roles.AnyAsync(x => x.Name == request.RoleName, cancellationToken);
        if (exists)
        {
            return Results.Ok();
        }

        writeContext.Roles.Add(request.ToEntity());
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AssignAccess(
        AssignAccessRequest request,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        Role? role = await writeContext.Roles.SingleOrDefaultAsync(x => x.Name == request.RoleName, cancellationToken);
        if (role is null)
        {
            return Results.NotFound(new { error = "Role not found" });
        }

        Permission? permission = await writeContext.Permissions.SingleOrDefaultAsync(x => x.Code == request.PermissionCode, cancellationToken);
        if (permission is null)
        {
            return Results.NotFound(new { error = "Permission not found" });
        }

        bool exists = await writeContext.RolePermissions.AnyAsync(
            x => x.RoleId == role.Id && x.PermissionId == permission.Id,
            cancellationToken);

        if (!exists)
        {
            writeContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            await writeContext.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private static IQueryable<LogEvent> ApplySorting(IQueryable<LogEvent> query, string? sortBy, string? sortOrder)
    {
        bool isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToUpperInvariant() switch
        {
            "LEVEL" => isDesc ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
            "SERVICE" => isDesc ? query.OrderByDescending(x => x.SourceService) : query.OrderBy(x => x.SourceService),
            _ => isDesc ? query.OrderByDescending(x => x.TimestampUtc) : query.OrderBy(x => x.TimestampUtc)
        };
    }

    private static LogEventView ToView(this LogEvent item, bool isCorrupted)
    {
        return new LogEventView
        {
            Id = item.Id,
            TimestampUtc = item.TimestampUtc,
            Level = item.Level,
            Message = item.Message,
            SourceService = item.SourceService,
            SourceModule = item.SourceModule,
            TraceId = item.TraceId,
            ActorId = item.ActorId,
            Outcome = item.Outcome,
            IsCorrupted = isCorrupted
        };
    }

    public sealed class LogEventView
    {
        public Guid Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public LogLevelType Level { get; set; }
        public string Message { get; set; }
        public string SourceService { get; set; }
        public string SourceModule { get; set; }
        public string TraceId { get; set; }
        public string? ActorId { get; set; }
        public string Outcome { get; set; }
        public bool IsCorrupted { get; set; }
    }

    public sealed class BulkIngestRequest
    {
        public List<IngestLogRequest> Events { get; set; } = [];
    }

    public sealed class CreateAlertRuleRequest
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public LogLevelType MinimumLevel { get; set; } = LogLevelType.Error;
        public string? ContainsText { get; set; }
        public int WindowSeconds { get; set; } = 60;
        public int ThresholdCount { get; set; } = 5;
        public string Action { get; set; } = "notification";
    }

    public sealed class CreateRoleRequest
    {
        public string RoleName { get; set; }
    }

    public sealed class AssignAccessRequest
    {
        public string RoleName { get; set; }
        public string PermissionCode { get; set; }
    }

    private static AlertRule ToEntity(this CreateAlertRuleRequest request)
    {
        return new AlertRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsEnabled = request.IsEnabled,
            MinimumLevel = request.MinimumLevel,
            ContainsText = request.ContainsText,
            WindowSeconds = request.WindowSeconds,
            ThresholdCount = request.ThresholdCount,
            Action = request.Action,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static void Update(this CreateAlertRuleRequest request, AlertRule rule)
    {
        rule.Name = request.Name;
        rule.IsEnabled = request.IsEnabled;
        rule.MinimumLevel = request.MinimumLevel;
        rule.ContainsText = request.ContainsText;
        rule.WindowSeconds = request.WindowSeconds;
        rule.ThresholdCount = request.ThresholdCount;
        rule.Action = request.Action;
        rule.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static Role ToEntity(this CreateRoleRequest request) =>
        new() { Id = Guid.NewGuid(), Name = request.RoleName };
}
