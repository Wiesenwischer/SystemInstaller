using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Users.Model;

/// <summary>
/// User preference for receiving notifications
/// </summary>
public class NotificationPreferences : ValueObject
{
    public bool EmailNotifications { get; }
    public bool InstallationUpdates { get; }
    public bool SystemMaintenance { get; }
    
    public NotificationPreferences(
        bool emailNotifications = true, 
        bool installationUpdates = true, 
        bool systemMaintenance = true)
    {
        EmailNotifications = emailNotifications;
        InstallationUpdates = installationUpdates;
        SystemMaintenance = systemMaintenance;
    }
    
    public static NotificationPreferences Default() => new();
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EmailNotifications;
        yield return InstallationUpdates;
        yield return SystemMaintenance;
    }
}
