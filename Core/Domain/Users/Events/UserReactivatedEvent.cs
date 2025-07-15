using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user is reactivated
/// </summary>
public class UserReactivatedEvent : DomainEvent
{
    public UserId UserId { get; }
    public DateTime ReactivatedAt { get; }
    
    public UserReactivatedEvent(UserId userId, DateTime reactivatedAt)
    {
        UserId = userId;
        ReactivatedAt = reactivatedAt;
    }
}
