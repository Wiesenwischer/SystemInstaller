using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when email verification is requested
/// </summary>
public class EmailVerificationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public EmailVerificationToken Token { get; }
    
    public EmailVerificationRequestedEvent(UserRegistrationId registrationId, Email email, EmailVerificationToken token)
    {
        RegistrationId = registrationId;
        Email = email;
        Token = token;
    }
}
