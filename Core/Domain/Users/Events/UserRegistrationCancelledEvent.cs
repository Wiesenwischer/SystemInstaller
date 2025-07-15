using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user registration is cancelled
/// </summary>
public class UserRegistrationCancelledEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }
    
    public UserRegistrationCancelledEvent(UserRegistrationId registrationId, Email email, string reason, DateTime cancelledAt)
    {
        RegistrationId = registrationId;
        Email = email;
        Reason = reason;
        CancelledAt = cancelledAt;
    }
}
