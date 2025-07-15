namespace SystemInstaller.Domain.Users.Model;

/// <summary>
/// Registration status enumeration
/// </summary>
public enum RegistrationStatus
{
    Pending,           // Initial state - registration requested
    EmailSent,         // Verification email sent
    EmailVerified,     // User clicked verification link
    ExternalUserCreated, // User created in Keycloak
    Completed,         // Registration fully completed
    Cancelled,         // Registration cancelled
    Expired            // Verification token expired
}
