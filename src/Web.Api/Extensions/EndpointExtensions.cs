using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services
            .GetRequiredService<IEnumerable<IEndpoint>>()
            .OrderBy(GetModuleName)
            .ThenBy(GetEndpointOrder)
            .ThenBy(endpoint => endpoint.GetType().Name, StringComparer.Ordinal);

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }

    private static string GetModuleName(IEndpoint endpoint)
    {
        string? endpointNamespace = endpoint.GetType().Namespace;
        if (string.IsNullOrWhiteSpace(endpointNamespace))
        {
            return "Core";
        }

        const string modulesMarker = ".Endpoints.Modules.";
        int modulesIndex = endpointNamespace.IndexOf(modulesMarker, StringComparison.Ordinal);
        if (modulesIndex >= 0)
        {
            string modulePath = endpointNamespace[(modulesIndex + modulesMarker.Length)..];
            string module = modulePath.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Core";
            return module;
        }

        const string endpointsMarker = ".Endpoints.";
        int endpointsIndex = endpointNamespace.IndexOf(endpointsMarker, StringComparison.Ordinal);
        if (endpointsIndex >= 0)
        {
            string modulePath = endpointNamespace[(endpointsIndex + endpointsMarker.Length)..];
            string module = modulePath.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Core";
            return module;
        }

        return "Core";
    }

    private static int GetEndpointOrder(IEndpoint endpoint)
    {
        return endpoint is IOrderedEndpoint orderedEndpoint
            ? orderedEndpoint.Order
            : int.MaxValue;
    }
}

