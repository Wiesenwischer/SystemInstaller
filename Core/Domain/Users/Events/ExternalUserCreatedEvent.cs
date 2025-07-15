using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when external user is created (e.g., in Keycloak)
/// </summary>
public class ExternalUserCreatedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public string ExternalUserId { get; }
    
    public ExternalUserCreatedEvent(UserRegistrationId registrationId, Email email, string externalUserId)
    {
        RegistrationId = registrationId;
        Email = email;
        ExternalUserId = externalUserId;
    }
}
