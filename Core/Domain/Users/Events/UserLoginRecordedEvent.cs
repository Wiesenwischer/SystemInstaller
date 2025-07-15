using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user login is recorded
/// </summary>
public class UserLoginRecordedEvent : DomainEvent
{
    public UserId UserId { get; }
    public DateTime LoginAt { get; }
    
    public UserLoginRecordedEvent(UserId userId, DateTime loginAt)
    {
        UserId = userId;
        LoginAt = loginAt;
    }
}
