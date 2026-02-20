using System.Net.Http.Json;

namespace ArchitectureTests.Modules.Common;

public sealed class EndpointAccessCase
{
    public EndpointAccessCase(string name, HttpMethod method, string path, object? body = null)
    {
        Name = name;
        Method = method;
        Path = path;
        Body = body;
    }

    public string Name { get; }

    public HttpMethod Method { get; }

    public string Path { get; }

    public object? Body { get; }

    public async Task<HttpResponseMessage> SendAsync(HttpClient client)
    {
        string resolvedPath = Path.Replace("{guid}", Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);

        if (Method == HttpMethod.Get)
        {
            return await client.GetAsync($"/api/v1/{resolvedPath}");
        }

        if (Method == HttpMethod.Delete)
        {
            return await client.DeleteAsync($"/api/v1/{resolvedPath}");
        }

        if (Method == HttpMethod.Post)
        {
            return await client.PostAsJsonAsync($"/api/v1/{resolvedPath}", Body ?? new { });
        }

        if (Method == HttpMethod.Put)
        {
            return await client.PutAsJsonAsync($"/api/v1/{resolvedPath}", Body ?? new { });
        }

        if (Method == HttpMethod.Patch)
        {
            using HttpRequestMessage request = new(HttpMethod.Patch, $"/api/v1/{resolvedPath}")
            {
                Content = JsonContent.Create(Body ?? new { })
            };
            return await client.SendAsync(request);
        }

        throw new InvalidOperationException($"Unsupported method {Method}.");
    }
}
