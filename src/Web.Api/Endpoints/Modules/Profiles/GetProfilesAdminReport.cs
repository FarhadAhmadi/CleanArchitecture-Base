using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed class ProfileAdminReportRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
    public bool? IsProfilePublic { get; set; }
    public string? PreferredLanguage { get; set; }
    public int? MinCompleteness { get; set; }
    public int? MaxCompleteness { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }
}

internal sealed class GetProfilesAdminReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/reports/admin", async (
                [AsParameters] ProfileAdminReportRequest request,
                IQueryHandler<GetProfilesAdminReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new GetProfilesAdminReportQuery(
                    request.Page,
                    request.PageIndex,
                    request.PageSize,
                    request.Search,
                    request.IsProfilePublic,
                    request.PreferredLanguage,
                    request.MinCompleteness,
                    request.MaxCompleteness,
                    request.UpdatedFrom,
                    request.UpdatedTo),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesAdminRead)
            .WithTags(Tags.Profiles);
    }
}

