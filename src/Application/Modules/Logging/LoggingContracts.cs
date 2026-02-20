using Domain.Authorization;
using Domain.Logging;
using Application.Abstractions.Data;
using Infrastructure.Logging;
using Application.Shared;

namespace Application.Logging;

public sealed class AssignAccessRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string PermissionCode { get; set; } = string.Empty;
}

public sealed class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}

public sealed class CreateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public LogLevelType MinimumLevel { get; set; } = LogLevelType.Error;
    public string? ContainsText { get; set; }
    public int WindowSeconds { get; set; } = 60;
    public int ThresholdCount { get; set; } = 5;
    public string Action { get; set; } = "notification";
}

public sealed class UpdateAlertRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public LogLevelType MinimumLevel { get; set; } = LogLevelType.Error;
    public string? ContainsText { get; set; }
    public int WindowSeconds { get; set; } = 60;
    public int ThresholdCount { get; set; } = 5;
    public string Action { get; set; } = "notification";
}

public sealed class GetAlertIncidentsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Status { get; set; }
}

public sealed class BulkIngestRequest
{
    public List<IngestLogRequest> Events { get; set; } = [];
}

public sealed class GetLogEventsRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public LogLevelType? Level { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? ActorId { get; set; }
    public string? Service { get; set; }
    public string? Module { get; set; }
    public string? TraceId { get; set; }
    public string? Outcome { get; set; }
    public string? Text { get; set; }
    public bool RecalculateIntegrity { get; set; }

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? 1;
        int pageSize = PageSize ?? 50;
        return QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
    }
}

public sealed class LogEventView
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public LogLevelType Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public bool IsCorrupted { get; set; }
    public bool HasIntegrityIssue { get; set; }
}

internal static class LoggingMappings
{
    internal static AlertRule ToEntity(this CreateAlertRuleRequest request)
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

    internal static void Update(this UpdateAlertRuleRequest request, AlertRule rule)
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

    internal static void Sanitize(this CreateAlertRuleRequest request)
    {
        request.Name = InputSanitizer.SanitizeText(request.Name, 200) ?? string.Empty;
        request.ContainsText = InputSanitizer.SanitizeText(request.ContainsText, 500);
        request.Action = InputSanitizer.SanitizeIdentifier(request.Action, 100) ?? "notification";
        request.WindowSeconds = Math.Max(1, request.WindowSeconds);
        request.ThresholdCount = Math.Max(1, request.ThresholdCount);
    }

    internal static void Sanitize(this UpdateAlertRuleRequest request)
    {
        request.Name = InputSanitizer.SanitizeText(request.Name, 200) ?? string.Empty;
        request.ContainsText = InputSanitizer.SanitizeText(request.ContainsText, 500);
        request.Action = InputSanitizer.SanitizeIdentifier(request.Action, 100) ?? "notification";
        request.WindowSeconds = Math.Max(1, request.WindowSeconds);
        request.ThresholdCount = Math.Max(1, request.ThresholdCount);
    }

    internal static Role ToEntity(this CreateRoleRequest request) =>
        new() { Id = Guid.NewGuid(), Name = request.RoleName };

    internal static RolePermission ToEntity(this AssignAccessRequest _, Guid roleId, Guid permissionId) =>
        new() { RoleId = roleId, PermissionId = permissionId };

    internal static LogEventView ToView(this LogEvent item, bool isCorrupted)
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

    internal static IQueryable<LogEvent> ApplySorting(this IQueryable<LogEvent> query, string? sortBy, string? sortOrder)
    {
        bool isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToUpperInvariant() switch
        {
            "LEVEL" => isDesc ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
            "SERVICE" => isDesc ? query.OrderByDescending(x => x.SourceService) : query.OrderBy(x => x.SourceService),
            _ => isDesc ? query.OrderByDescending(x => x.TimestampUtc) : query.OrderBy(x => x.TimestampUtc)
        };
    }
}




