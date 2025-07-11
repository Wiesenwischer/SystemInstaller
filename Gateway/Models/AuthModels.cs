namespace SystemInstaller.Gateway.Models;

public record LoginRequest(string Username, string Password);

public record LoginResponse(bool Success, string? Message = null);

public record UserInfo(
    string Username, 
    string Email, 
    string FirstName, 
    string LastName, 
    List<string> Roles
);

public record AuthResult(
    bool IsAuthenticated,
    UserInfo? User = null,
    string? AccessToken = null,
    string? RefreshToken = null
);
