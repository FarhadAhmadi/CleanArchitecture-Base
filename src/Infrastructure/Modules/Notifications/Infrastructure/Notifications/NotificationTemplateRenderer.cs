using System.Text.RegularExpressions;
using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Notifications;

public sealed partial class NotificationTemplateRenderer(ApplicationDbContext dbContext)
{
    public async Task<RenderedNotificationTemplate?> TryRenderAsync(
        string templateName,
        string language,
        NotificationChannel channel,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await dbContext.NotificationTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted &&
                        x.Name == templateName &&
                        x.Language == language &&
                        x.Channel == channel)
            .SingleOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return null;
        }

        return new RenderedNotificationTemplate(
            template.Id,
            ReplaceTokens(template.SubjectTemplate, variables),
            ReplaceTokens(template.BodyTemplate, variables));
    }

    private static string ReplaceTokens(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return TokenRegex().Replace(template, match =>
        {
            string key = match.Groups["key"].Value;
            return variables.TryGetValue(key, out string? value) ? value : match.Value;
        });
    }

    [GeneratedRegex(@"\{\{\s*(?<key>[a-zA-Z0-9_.-]+)\s*\}\}", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();
}

public sealed record RenderedNotificationTemplate(Guid TemplateId, string Subject, string Body);
