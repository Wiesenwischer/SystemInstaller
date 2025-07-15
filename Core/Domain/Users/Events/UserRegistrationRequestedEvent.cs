using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when a user requests registration
/// </summary>
public class UserRegistrationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public PersonName Name { get; }
    
    public UserRegistrationRequestedEvent(UserRegistrationId registrationId, Email email, PersonName name)
    {
        RegistrationId = registrationId;
        Email = email;
        Name = name;
    }
}
