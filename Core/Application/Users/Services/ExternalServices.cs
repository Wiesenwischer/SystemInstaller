namespace SystemInstaller.Application.Users.Services;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string firstName, string verificationToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default);
    Task SendRegistrationCancelledEmailAsync(string email, string firstName, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for integrating with Keycloak identity provider
/// </summary>
public interface IKeycloakService
{
    Task<string> CreateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateUserAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for URL generation
/// </summary>
public interface IUrlGenerationService
{
    string GenerateEmailVerificationUrl(string token);
    string GeneratePasswordResetUrl(string token);
    string GenerateAccountActivationUrl(string token);
}
