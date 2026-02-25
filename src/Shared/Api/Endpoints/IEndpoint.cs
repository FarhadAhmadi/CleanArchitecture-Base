namespace Web.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public interface IOrderedEndpoint
{
    int Order { get; }
}

