using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Authorization;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Security;

internal static class TestIdentityHelper
{
    internal static async Task<(Guid UserId, string AccessToken, string RefreshToken)> RegisterLoginAndAssignRoleAsync(
        ApiWebApplicationFactory factory,
        HttpClient client,
        string roleName)
    {
        string email = $"user-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Test",
            lastName = "User",
            password
        });

        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        Guid userId = await register.Content.ReadFromJsonAsync<Guid>();

        await AttachRoleAsync(factory, userId, roleName);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        (string accessToken, string refreshToken) tokens = await ReadTokensAsync(login);
        await ApiWebApplicationFactory.SetBearerTokenAsync(client, tokens.accessToken);

        return (userId, tokens.accessToken, tokens.refreshToken);
    }

    internal static async Task AttachRoleAsync(ApiWebApplicationFactory factory, Guid userId, string roleName)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Guid roleId = await dbContext.Roles
            .Where(x => x.Name == roleName)
            .Select(x => x.Id)
            .SingleAsync();

        bool exists = await dbContext.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId);
        if (!exists)
        {
            dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await dbContext.SaveChangesAsync();
        }
    }

    internal static async Task<(string AccessToken, string RefreshToken)> ReadTokensAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        string accessToken = document.RootElement.GetProperty("accessToken").GetString()!;
        string refreshToken = document.RootElement.GetProperty("refreshToken").GetString()!;
        return (accessToken, refreshToken);
    }
}
