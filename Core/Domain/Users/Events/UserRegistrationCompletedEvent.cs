using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user registration is completed
/// </summary>
public class UserRegistrationCompletedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public UserId UserId { get; }
    public Email Email { get; }
    public PersonName Name { get; }
    public string ExternalUserId { get; }
    public DateTime CompletedAt { get; }
    
    public UserRegistrationCompletedEvent(
        UserRegistrationId registrationId, 
        UserId userId, 
        Email email, 
        PersonName name, 
        string externalUserId, 
        DateTime completedAt)
    {
        RegistrationId = registrationId;
        UserId = userId;
        Email = email;
        Name = name;
        ExternalUserId = externalUserId;
        CompletedAt = completedAt;
    }
}
