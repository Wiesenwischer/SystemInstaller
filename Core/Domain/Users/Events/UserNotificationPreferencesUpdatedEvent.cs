using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Users.Model;

namespace SystemInstaller.Domain.Users.Events;

/// <summary>
/// Domain event raised when user notification preferences are updated
/// </summary>
public class UserNotificationPreferencesUpdatedEvent : DomainEvent
{
    public UserId UserId { get; }
    public NotificationPreferences Preferences { get; }
    
    public UserNotificationPreferencesUpdatedEvent(UserId userId, NotificationPreferences preferences)
    {
        UserId = userId;
        Preferences = preferences;
    }
}
