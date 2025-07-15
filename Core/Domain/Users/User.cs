using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants; // For Email and PersonName

namespace SystemInstaller.Domain.Users;

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

/// <summary>
/// Additional domain events for User entity
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
