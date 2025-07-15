using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public class UserCreatedEvent : DomainEvent
{
    public UserId UserId { get; }
    public Email Email { get; }
    public PersonName Name { get; }
    public string ExternalUserId { get; }
    
    public UserCreatedEvent(UserId userId, Email email, PersonName name, string externalUserId)
    {
        UserId = userId;
        Email = email;
        Name = name;
        ExternalUserId = externalUserId;
    }
}
