using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Web.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((swaggerDocument, httpRequest) =>
            {
                swaggerDocument.Servers =
                [
                    new OpenApiServer
                    {
                        Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}"
                    }
                ];
            });
        });

        app.UseSwaggerUI(options =>
        {
            options.DocExpansion(DocExpansion.None);
        });

        return app;
    }
}
