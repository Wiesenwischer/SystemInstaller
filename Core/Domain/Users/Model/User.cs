using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model; // For Email and PersonName
using SystemInstaller.Domain.Users.Events;

namespace SystemInstaller.Domain.Users.Model;

/// <summary>
/// User entity - represents a fully registered and verified user
/// </summary>
public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; } = default!;
    public PersonName Name { get; private set; } = default!;
    public string ExternalUserId { get; private set; } = default!; // Keycloak user ID
    public NotificationPreferences NotificationPreferences { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    
    // Navigation properties
    public UserRegistration? Registration { get; private set; }
    
    private User() { } // For EF Core
    
    internal User(Email email, PersonName name, string externalUserId, NotificationPreferences notificationPreferences)
    {
        Id = UserId.New();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ExternalUserId = externalUserId ?? throw new ArgumentNullException(nameof(externalUserId));
        NotificationPreferences = notificationPreferences ?? throw new ArgumentNullException(nameof(notificationPreferences));
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserCreatedEvent(Id, Email, Name, ExternalUserId));
    }
    
    public void UpdateProfile(PersonName name)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Cannot update profile of inactive user");
        
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new UserProfileUpdatedEvent(Id, Name));
    }
    
    public void UpdateNotificationPreferences(NotificationPreferences preferences)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Cannot update preferences of inactive user");
        
        NotificationPreferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new UserNotificationPreferencesUpdatedEvent(Id, preferences));
    }
    
    public void RecordLogin()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("Cannot record login for inactive user");
        
        LastLoginAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new UserLoginRecordedEvent(Id, LastLoginAt.Value));
    }
    
    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("User is already deactivated");
        
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new UserDeactivatedEvent(Id, reason, DeactivatedAt.Value));
    }
    
    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleViolationException("User is already active");
        
        IsActive = true;
        DeactivatedAt = null;
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new UserReactivatedEvent(Id, UpdatedAt.Value));
    }
}
