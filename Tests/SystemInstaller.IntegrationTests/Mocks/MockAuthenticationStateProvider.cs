using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SystemInstaller.IntegrationTests.Mocks;

public class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;

    public MockAuthenticationStateProvider(ClaimsPrincipal user)
    {
        _user = user;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_user));
    }

    public void TriggerAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

public static class MockUsers
{
    public static ClaimsPrincipal CreateAuthenticatedUser(
        string userId = "test-user-id",
        string email = "test@example.com",
        string name = "Test User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new("preferred_username", email)
        };

        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    public static ClaimsPrincipal CreateAdminUser(
        string userId = "admin-user-id",
        string email = "admin@example.com",
        string name = "Admin User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, "Admin"),
            new("preferred_username", email)
        };

        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }
}
