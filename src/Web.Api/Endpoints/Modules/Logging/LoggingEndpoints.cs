using System.Security.Claims;
using Application.Abstractions.Data;
using Domain.Authorization;
using Domain.Logging;
using Infrastructure.Auditing;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            .WithTags("Logging")
            .AddEndpointFilterFactory(EndpointExecutionLoggingFilter.Create)
            .AddEndpointFilterFactory(RequestSanitizationEndpointFilter.Create);

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
        string? idempotencyKey = InputSanitizer.SanitizeIdentifier(
            httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault(),
            120);
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
            string? key = InputSanitizer.SanitizeIdentifier(
                httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault(),
                100);
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
        IApplicationDbContext writeContext,
        ILogIntegrityService integrityService,
        ILoggerFactory loggerFactory,
        [AsParameters] GetLogEventsRequest request,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("LoggingEndpoints.GetEvents");

        (int normalizedPage, int normalizedPageSize) = request.NormalizePaging();

        IQueryable<LogEvent> query = readContext.LogEvents.Where(x => !x.IsDeleted);

        if (request.Level.HasValue)
        {
            query = query.Where(x => x.Level == request.Level.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(x => x.TimestampUtc >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(x => x.TimestampUtc <= request.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ActorId))
        {
            query = query.Where(x => x.ActorId == request.ActorId);
        }

        if (!string.IsNullOrWhiteSpace(request.Service))
        {
            query = query.Where(x => x.SourceService == request.Service);
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            query = query.Where(x => x.SourceModule == request.Module);
        }

        if (!string.IsNullOrWhiteSpace(request.TraceId))
        {
            query = query.Where(x => x.TraceId == request.TraceId);
        }

        if (!string.IsNullOrWhiteSpace(request.Outcome))
        {
            query = query.Where(x => x.Outcome == request.Outcome);
        }

        query = query.ApplyContainsSearch(request.Text, x => x.Message, x => x.PayloadJson, x => x.TagsCsv);

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        int total = await query.CountAsync(cancellationToken);
        List<LogEvent> items = await query
            .ApplyPaging(normalizedPage, normalizedPageSize)
            .ToListAsync(cancellationToken);

        List<Guid> corruptedIds = [];
        List<LogEventView> resultItems = [];

        foreach (LogEvent item in items)
        {
            bool isCorrupted = integrityService.IsCorrupted(item) || item.HasIntegrityIssue;
            if (isCorrupted && !item.HasIntegrityIssue)
            {
                corruptedIds.Add(item.Id);
            }

            resultItems.Add(item.ToView(isCorrupted));
        }

        if (request.RecalculateIntegrity && corruptedIds.Count != 0)
        {
            List<LogEvent> trackedItems = await writeContext.LogEvents
                .Where(x => corruptedIds.Contains(x.Id) && !x.HasIntegrityIssue)
                .ToListAsync(cancellationToken);

            foreach (LogEvent trackedItem in trackedItems)
            {
                trackedItem.HasIntegrityIssue = true;
            }

            await writeContext.SaveChangesAsync(cancellationToken);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Log events queried. Total={Total} Returned={Returned} Page={Page} PageSize={PageSize} Level={Level}",
                total,
                resultItems.Count,
                normalizedPage,
                normalizedPageSize,
                request.Level);
        }

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
        ILoggerFactory loggerFactory,
        bool recalculate,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("LoggingEndpoints.GetCorruptedEvents");

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

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Corrupted log events queried. TotalChecked={TotalChecked} Corrupted={Corrupted} Recalculated={Recalculated}",
                events.Count,
                corrupted.Count,
                recalculate);
        }

        return Results.Ok(new { total = corrupted.Count, items = corrupted });
    }

    private static async Task<IResult> GetEventById(
        Guid eventId,
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        ILogIntegrityService integrityService,
        bool recalculateIntegrity,
        CancellationToken cancellationToken)
    {
        LogEvent? item = await readContext.LogEvents
            .SingleOrDefaultAsync(x => x.Id == eventId && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        bool isCorrupted = integrityService.IsCorrupted(item) || item.HasIntegrityIssue;
        if (recalculateIntegrity && isCorrupted && !item.HasIntegrityIssue)
        {
            LogEvent? tracked = await writeContext.LogEvents.SingleOrDefaultAsync(x => x.Id == item.Id, cancellationToken);
            if (tracked is not null)
            {
                tracked.HasIntegrityIssue = true;
                await writeContext.SaveChangesAsync(cancellationToken);
            }
        }

        return Results.Ok(item.ToView(isCorrupted));
    }

    private static async Task<IResult> DeleteEvent(
        Guid eventId,
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
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

        string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.event.delete",
                "LogEvent",
                eventId.ToString("N"),
                "{\"softDelete\":true}"),
            cancellationToken);

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
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        request.Sanitize();
        AlertRule entity = request.ToEntity();
        writeContext.AlertRules.Add(entity);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.create",
                "AlertRule",
                entity.Id.ToString("N"),
                $"{{\"name\":\"{entity.Name}\",\"minimumLevel\":\"{entity.MinimumLevel}\"}}"),
            cancellationToken);

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
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        request.Sanitize();
        AlertRule? rule = await writeContext.AlertRules.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (rule is null)
        {
            return Results.NotFound();
        }

        request.Update(rule);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.update",
                "AlertRule",
                rule.Id.ToString("N"),
                $"{{\"name\":\"{rule.Name}\",\"minimumLevel\":\"{rule.MinimumLevel}\"}}"),
            cancellationToken);

        return Results.Ok(rule);
    }

    private static async Task<IResult> DeleteRule(
        Guid id,
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
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

        string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.delete",
                "AlertRule",
                id.ToString("N"),
                "{}"),
            cancellationToken);

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
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        request.RoleName = InputSanitizer.SanitizeIdentifier(request.RoleName, 100) ?? string.Empty;
        bool exists = await writeContext.Roles.AnyAsync(x => x.Name == request.RoleName, cancellationToken);
        if (exists)
        {
            return Results.Ok();
        }

        Role role = request.ToEntity();
        writeContext.Roles.Add(role);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.access-role.create",
                "Role",
                role.Id.ToString("N"),
                $"{{\"name\":\"{role.Name}\"}}"),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AssignAccess(
        AssignAccessRequest request,
        HttpContext httpContext,
        IAuditTrailService auditTrailService,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        request.RoleName = InputSanitizer.SanitizeIdentifier(request.RoleName, 100) ?? string.Empty;
        request.PermissionCode = InputSanitizer.SanitizeIdentifier(request.PermissionCode, 200) ?? string.Empty;

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
            writeContext.RolePermissions.Add(request.ToEntity(role.Id, permission.Id));
            await writeContext.SaveChangesAsync(cancellationToken);

            string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            await auditTrailService.RecordAsync(
                new AuditRecordRequest(
                    actorId,
                    "logging.access.assign",
                    "RolePermission",
                    role.Id.ToString("N"),
                    $"{{\"permission\":\"{permission.Code}\"}}"),
                cancellationToken);
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
            IsCorrupted = isCorrupted,
            HasIntegrityIssue = item.HasIntegrityIssue
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
        public bool HasIntegrityIssue { get; set; }
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

    private static void Sanitize(this CreateAlertRuleRequest request)
    {
        request.Name = InputSanitizer.SanitizeText(request.Name, 200) ?? string.Empty;
        request.ContainsText = InputSanitizer.SanitizeText(request.ContainsText, 500);
        request.Action = InputSanitizer.SanitizeIdentifier(request.Action, 100) ?? "notification";
        request.WindowSeconds = Math.Max(1, request.WindowSeconds);
        request.ThresholdCount = Math.Max(1, request.ThresholdCount);
    }

    private static Role ToEntity(this CreateRoleRequest request) =>
        new() { Id = Guid.NewGuid(), Name = request.RoleName };

    private static RolePermission ToEntity(this AssignAccessRequest _, Guid roleId, Guid permissionId) =>
        new() { RoleId = roleId, PermissionId = permissionId };
}
