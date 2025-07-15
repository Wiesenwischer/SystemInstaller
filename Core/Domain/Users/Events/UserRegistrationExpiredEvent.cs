using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user registration expires
/// </summary>
public class UserRegistrationExpiredEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public DateTime ExpiredAt { get; }
    
    public UserRegistrationExpiredEvent(UserRegistrationId registrationId, Email email, DateTime expiredAt)
    {
        RegistrationId = registrationId;
        Email = email;
        ExpiredAt = expiredAt;
    }
}
