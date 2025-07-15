using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Users;

/// <summary>
/// Domain events for user registration process
/// </summary>
public class UserRegistrationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public PersonName Name { get; }
    
    public UserRegistrationRequestedEvent(UserRegistrationId registrationId, Email email, PersonName name)
    {
        RegistrationId = registrationId;
        Email = email;
        Name = name;
    }
}

public class EmailVerificationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public EmailVerificationToken Token { get; }
    
    public EmailVerificationRequestedEvent(UserRegistrationId registrationId, Email email, EmailVerificationToken token)
    {
        RegistrationId = registrationId;
        Email = email;
        Token = token;
    }
}

public class EmailVerifiedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public DateTime VerifiedAt { get; }
    
    public EmailVerifiedEvent(UserRegistrationId registrationId, Email email, DateTime verifiedAt)
    {
        RegistrationId = registrationId;
        Email = email;
        VerifiedAt = verifiedAt;
    }
}

public class ExternalUserCreatedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public string ExternalUserId { get; }
    
    public ExternalUserCreatedEvent(UserRegistrationId registrationId, Email email, string externalUserId)
    {
        RegistrationId = registrationId;
        Email = email;
        ExternalUserId = externalUserId;
    }
}

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

public class UserRegistrationCancelledEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }
    
    public UserRegistrationCancelledEvent(UserRegistrationId registrationId, Email email, string reason, DateTime cancelledAt)
    {
        RegistrationId = registrationId;
        Email = email;
        Reason = reason;
        CancelledAt = cancelledAt;
    }
}

public class UserRegistrationExpiredEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public DateTime ExpiredAt { get; }
    
    public UserRegistrationExpiredEvent(UserRegistrationId registrationId, Email email, DateTime expiredAt)
    {
        RegistrationId = registrationId;
        Email = email;
        ExpiredAt = expiredAt;
    }
}
