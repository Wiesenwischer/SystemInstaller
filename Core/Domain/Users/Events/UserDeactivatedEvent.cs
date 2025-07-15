using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user is deactivated
/// </summary>
public class UserDeactivatedEvent : DomainEvent
{
    public UserId UserId { get; }
    public string Reason { get; }
    public DateTime DeactivatedAt { get; }
    
    public UserDeactivatedEvent(UserId userId, string reason, DateTime deactivatedAt)
    {
        UserId = userId;
        Reason = reason;
        DeactivatedAt = deactivatedAt;
    }
}
