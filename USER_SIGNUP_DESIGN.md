# ReadyStackGo (RSGO) User Sign-Up Design

<div align="center">
  <img src="assets/logo.png" alt="ReadyStackGo Logo" width="300">
</div>

> **Turn your specs into stacks**

## Overview

This document describes the domain-driven design for user sign-up functionality in the ReadyStackGo (RSGO) application. The process involves a multi-step workflow with email verification and external identity provider integration.

## Business Process

### Sign-Up Flow

1. **User Registration Request**
   - User fills out registration form
   - Basic validation (email format, required fields)
   - Check if user already exists

2. **Pending User Creation**
   - Create `PendingUser` entity in domain
   - Generate email verification token
   - Store registration details temporarily

3. **Email Verification**
   - Send verification email with secure token
   - Token has expiration time (e.g., 24 hours)
   - User clicks verification link

4. **External Identity Provider Integration**
   - Verify token and retrieve pending user
   - Create user account in Keycloak
   - Handle potential Keycloak errors

5. **User Account Activation**
   - Mark user as verified in domain
   - Convert `PendingUser` to `User`
   - Send welcome email

## Domain Model

### User Registration Aggregate

The `UserRegistration` aggregate manages the complete sign-up lifecycle:

```csharp
public class UserRegistration : AggregateRoot<UserRegistrationId>
{
    public Email Email { get; private set; }
    public PersonName Name { get; private set; }
    public RegistrationStatus Status { get; private set; }
    public EmailVerificationToken VerificationToken { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ExternalUserId { get; private set; } // Keycloak user ID
    
    // Business methods
    public void RequestEmailVerification()
    public void VerifyEmail(string token)
    public void CompleteRegistration(string externalUserId)
    public void CancelRegistration(string reason)
}
```

### Value Objects

**EmailVerificationToken**
```csharp
public class EmailVerificationToken : ValueObject
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsExpired;
}
```

**RegistrationStatus**
```csharp
public enum RegistrationStatus
{
    Pending,           // Initial state
    EmailSent,         // Verification email sent
    EmailVerified,     // User clicked verification link
    ExternalUserCreated, // User created in Keycloak
    Completed,         // Registration fully completed
    Cancelled,         // Registration cancelled
    Expired            // Verification token expired
}
```

### Domain Events

```csharp
public class UserRegistrationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public PersonName Name { get; }
}

public class EmailVerificationRequestedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public EmailVerificationToken Token { get; }
}

public class EmailVerifiedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public DateTime VerifiedAt { get; }
}

public class UserRegistrationCompletedEvent : DomainEvent
{
    public UserRegistrationId RegistrationId { get; }
    public Email Email { get; }
    public string ExternalUserId { get; }
    public DateTime CompletedAt { get; }
}
```

## Application Layer

### Use Cases

#### 1. Request User Registration
```csharp
public class RequestUserRegistrationCommand : IRequest<RequestUserRegistrationResult>
{
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}

public class RequestUserRegistrationHandler : IRequestHandler<RequestUserRegistrationCommand, RequestUserRegistrationResult>
{
    // Implementation
}
```

#### 2. Verify Email
```csharp
public class VerifyEmailCommand : IRequest<VerifyEmailResult>
{
    public string Token { get; set; } = default!;
}

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    // Implementation
}
```

#### 3. Complete Registration
```csharp
public class CompleteRegistrationCommand : IRequest<CompleteRegistrationResult>
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public string ExternalUserId { get; set; } = default!;
}

public class CompleteRegistrationHandler : IRequestHandler<CompleteRegistrationCommand, CompleteRegistrationResult>
{
    // Implementation
}
```

### Domain Event Handlers

#### Email Verification Handler
```csharp
public class EmailVerificationRequestedHandler : INotificationHandler<EmailVerificationRequestedEvent>
{
    private readonly IEmailService _emailService;
    
    public async Task Handle(EmailVerificationRequestedEvent notification, CancellationToken cancellationToken)
    {
        // Send verification email
        await _emailService.SendVerificationEmailAsync(
            notification.Email.Value,
            notification.Token.Token,
            cancellationToken);
    }
}
```

#### User Creation Handler
```csharp
public class EmailVerifiedHandler : INotificationHandler<EmailVerifiedEvent>
{
    private readonly IKeycloakService _keycloakService;
    private readonly IMediator _mediator;
    
    public async Task Handle(EmailVerifiedEvent notification, CancellationToken cancellationToken)
    {
        // Create user in Keycloak
        var externalUserId = await _keycloakService.CreateUserAsync(
            notification.Email.Value,
            // ... other parameters
            cancellationToken);
        
        // Complete registration
        await _mediator.Send(new CompleteRegistrationCommand 
        { 
            RegistrationId = notification.RegistrationId,
            ExternalUserId = externalUserId
        }, cancellationToken);
    }
}
```

## Infrastructure Services

### Email Service
```csharp
public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken = default);
}
```

### Keycloak Service
```csharp
public interface IKeycloakService
{
    Task<string> CreateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}
```

## Security Considerations

### Token Security
- Use cryptographically secure random tokens
- Tokens should be long enough to prevent brute force attacks
- Implement rate limiting for verification attempts
- Tokens expire after reasonable time (24 hours)

### Email Security
- Use HTTPS for all verification links
- Include token in URL path, not query parameters
- Implement CSRF protection
- Log all verification attempts

## Error Handling

### Business Rule Violations
- User already exists
- Invalid email format
- Token expired
- Token already used
- External service unavailable

### Technical Errors
- Email service failures
- Keycloak service failures
- Database transaction failures
- Network timeouts

## Testing Strategy

### Unit Tests
- Domain aggregate behavior
- Value object validation
- Business rule enforcement
- Event generation

### Integration Tests
- Application use case flows
- External service integration
- Email template generation
- Database persistence

### End-to-End Tests
- Complete registration flow
- Error scenarios
- Email verification process
- User experience flows

## Monitoring and Observability

### Metrics
- Registration request rate
- Email verification success rate
- Keycloak user creation success rate
- Time to complete registration

### Logging
- All registration attempts
- Email verification attempts
- External service calls
- Error conditions

### Alerts
- High registration failure rate
- Email service unavailable
- Keycloak service issues
- Token expiration issues

## Implementation Plan

### Phase 1: Core Domain Model
1. Create UserRegistration aggregate
2. Implement value objects
3. Define domain events
4. Create repository interfaces

### Phase 2: Application Layer
1. Implement use case handlers
2. Create domain event handlers
3. Add validation logic
4. Implement error handling

### Phase 3: Infrastructure
1. Implement email service
2. Implement Keycloak service
3. Create EF Core configurations
4. Add external service integrations

### Phase 4: API and UI
1. Create API endpoints
2. Implement registration forms
3. Create verification pages
4. Add user feedback

### Phase 5: Testing and Deployment
1. Unit and integration tests
2. End-to-end testing
3. Performance testing
4. Security testing
5. Deployment and monitoring
