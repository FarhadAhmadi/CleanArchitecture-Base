using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Logging;

public static class LoggingEndpoints
{
    public static IEndpointRouteBuilder MapLoggingEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/logging/v1")
            .WithTags("Logging")
            .AddEndpointFilterFactory(EndpointExecutionLoggingFilter.Create)
            .AddEndpointFilterFactory(RequestSanitizationEndpointFilter.Create);

        group.MapLoggingIngestEvent();
        group.MapLoggingIngestBulkEvents();
        group.MapLoggingGetEvents();
        group.MapLoggingGetCorruptedEvents();
        group.MapLoggingGetEventById();
        group.MapLoggingDeleteEvent();
        group.MapLoggingGetSchema();
        group.MapLoggingValidateInput();
        group.MapLoggingTransformInput();
        group.MapLoggingHealth();

        group.MapLoggingCreateAlertRule();
        group.MapLoggingGetAlertRules();
        group.MapLoggingUpdateAlertRule();
        group.MapLoggingDeleteAlertRule();
        group.MapLoggingGetAlertIncidents();
        group.MapLoggingGetAlertIncidentById();

        group.MapLoggingGetAccessControl();
        group.MapLoggingCreateRole();
        group.MapLoggingAssignAccess();

        return app;
    }
}




















