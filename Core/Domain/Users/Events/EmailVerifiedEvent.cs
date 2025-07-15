using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when email is verified
/// </summary>
public class EmailVerifiedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public DateTime VerifiedAt { get; }
    
    public EmailVerifiedEvent(UserRegistrationId registrationId, Email email, DateTime verifiedAt)
    {
        RegistrationId = registrationId;
        Email = email;
        VerifiedAt = verifiedAt;
    }
}
