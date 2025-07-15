using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user profile is updated
/// </summary>
public class UserProfileUpdatedEvent : DomainEvent
{
    public UserId UserId { get; }
    public PersonName Name { get; }
    
    public UserProfileUpdatedEvent(UserId userId, PersonName name)
    {
        UserId = userId;
        Name = name;
    }
}
