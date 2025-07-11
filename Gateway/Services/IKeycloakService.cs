using SystemInstaller.Gateway.Models;

namespace SystemInstaller.Gateway.Services;

public interface IKeycloakService
{
    Task<AuthResult> AuthenticateAsync(string username, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<UserInfo?> GetUserInfoAsync(string accessToken);
    Task<bool> ValidateTokenAsync(string accessToken);
}
