using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetProfilesAdminReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/reports/admin", async (
                [AsParameters] ProfileAdminReportRequest request,
                IQueryHandler<GetProfilesAdminReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetProfilesAdminReportQuery(request), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesAdminRead)
            .WithTags(Tags.Profiles);
    }
}

