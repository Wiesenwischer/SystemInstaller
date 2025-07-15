using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants; // For Email and PersonName

namespace SystemInstaller.Domain.Users;

/// <summary>
/// UserRegistration aggregate root - manages the complete user sign-up lifecycle
/// </summary>
public class UserRegistration : AggregateRoot<UserRegistrationId>
{
    public Email Email { get; private set; } = default!;
    public PersonName Name { get; private set; } = default!;
    public RegistrationStatus Status { get; private set; }
    public EmailVerificationToken? VerificationToken { get; private set; }
    public NotificationPreferences NotificationPreferences { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? EmailSentAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? ExternalUserId { get; private set; } // Keycloak user ID
    public string? CancellationReason { get; private set; }
    public int VerificationAttempts { get; private set; }
    
    // Navigation properties
    public User? User { get; private set; } // Set when registration completes
    
    private UserRegistration() { } // For EF Core
    
    public UserRegistration(Email email, PersonName name, NotificationPreferences? preferences = null)
    {
        Id = UserRegistrationId.New();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Status = RegistrationStatus.Pending;
        NotificationPreferences = preferences ?? NotificationPreferences.Default();
        CreatedAt = DateTime.UtcNow;
        VerificationAttempts = 0;
        
        AddDomainEvent(new UserRegistrationRequestedEvent(Id, Email, Name));
    }
    
    public void SendVerificationEmail(TimeSpan? tokenValidFor = null)
    {
        if (Status != RegistrationStatus.Pending)
            throw new BusinessRuleViolationException($"Cannot send verification email in status: {Status}");
        
        VerificationToken = EmailVerificationToken.Generate(tokenValidFor);
        Status = RegistrationStatus.EmailSent;
        EmailSentAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new EmailVerificationRequestedEvent(Id, Email, VerificationToken));
    }
    
    public void VerifyEmail(string token)
    {
        if (Status != RegistrationStatus.EmailSent)
            throw new BusinessRuleViolationException($"Cannot verify email in status: {Status}");
        
        if (VerificationToken == null)
            throw new BusinessRuleViolationException("No verification token found");
        
        if (VerificationToken.Token != token)
        {
            VerificationAttempts++;
            IncrementVersion();
            
            // Too many failed attempts
            if (VerificationAttempts >= 5)
            {
                CancelRegistration("Too many failed verification attempts");
                return;
            }
            
            throw new BusinessRuleViolationException("Invalid verification token");
        }
        
        if (VerificationToken.IsExpired)
        {
            Status = RegistrationStatus.Expired;
            IncrementVersion();
            
            AddDomainEvent(new UserRegistrationExpiredEvent(Id, Email, DateTime.UtcNow));
            throw new BusinessRuleViolationException("Verification token has expired");
        }
        
        Status = RegistrationStatus.EmailVerified;
        VerifiedAt = DateTime.UtcNow;
        IncrementVersion();
        
        AddDomainEvent(new EmailVerifiedEvent(Id, Email, VerifiedAt.Value));
    }
    
    public void MarkExternalUserCreated(string externalUserId)
    {
        if (Status != RegistrationStatus.EmailVerified)
            throw new BusinessRuleViolationException($"Cannot mark external user created in status: {Status}");
        
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be null or empty", nameof(externalUserId));
        
        ExternalUserId = externalUserId;
        Status = RegistrationStatus.ExternalUserCreated;
        IncrementVersion();
        
        AddDomainEvent(new ExternalUserCreatedEvent(Id, Email, externalUserId));
    }
    
    public User CompleteRegistration()
    {
        if (Status != RegistrationStatus.ExternalUserCreated)
            throw new BusinessRuleViolationException($"Cannot complete registration in status: {Status}");
        
        if (string.IsNullOrEmpty(ExternalUserId))
            throw new BusinessRuleViolationException("External user ID is required to complete registration");
        
        Status = RegistrationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        // Create the domain User entity
        User = new User(Email, Name, ExternalUserId, NotificationPreferences);
        
        IncrementVersion();
        
        AddDomainEvent(new UserRegistrationCompletedEvent(
            Id, 
            User.Id, 
            Email, 
            Name, 
            ExternalUserId, 
            CompletedAt.Value));
        
        return User;
    }
    
    public void CancelRegistration(string reason)
    {
        if (Status == RegistrationStatus.Completed)
            throw new BusinessRuleViolationException("Cannot cancel completed registration");
        
        if (Status == RegistrationStatus.Cancelled)
            throw new BusinessRuleViolationException("Registration is already cancelled");
        
        Status = RegistrationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        IncrementVersion();
        
        AddDomainEvent(new UserRegistrationCancelledEvent(Id, Email, reason, CancelledAt.Value));
    }
    
    public void ResendVerificationEmail(TimeSpan? tokenValidFor = null)
    {
        if (Status != RegistrationStatus.EmailSent && Status != RegistrationStatus.Expired)
            throw new BusinessRuleViolationException($"Cannot resend verification email in status: {Status}");
        
        // Generate new token
        VerificationToken = EmailVerificationToken.Generate(tokenValidFor);
        Status = RegistrationStatus.EmailSent;
        EmailSentAt = DateTime.UtcNow;
        VerificationAttempts = 0; // Reset attempts
        IncrementVersion();
        
        AddDomainEvent(new EmailVerificationRequestedEvent(Id, Email, VerificationToken));
    }
    
    public bool CanResendEmail()
    {
        return Status == RegistrationStatus.EmailSent || Status == RegistrationStatus.Expired;
    }
    
    public bool IsExpired()
    {
        return VerificationToken?.IsExpired ?? false;
    }
    
    public void CheckForExpiration()
    {
        if (Status == RegistrationStatus.EmailSent && IsExpired())
        {
            Status = RegistrationStatus.Expired;
            IncrementVersion();
            
            AddDomainEvent(new UserRegistrationExpiredEvent(Id, Email, DateTime.UtcNow));
        }
    }
}
