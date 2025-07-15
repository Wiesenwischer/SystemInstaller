using MediatR;
using SystemInstaller.Application.Users.Services;
using SystemInstaller.Application.Users.SendVerificationEmail;
using SystemInstaller.Application.Users.CompleteRegistration;
using SystemInstaller.Domain.Users;

namespace SystemInstaller.Application.Users.EventHandlers;

/// <summary>
/// Handler for UserRegistrationRequestedEvent - sends verification email
/// </summary>
public class UserRegistrationRequestedHandler : INotificationHandler<UserRegistrationRequestedEvent>
{
    private readonly IMediator _mediator;

    public UserRegistrationRequestedHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserRegistrationRequestedEvent notification, CancellationToken cancellationToken)
    {
        // Automatically send verification email when registration is requested
        await _mediator.Send(new SendVerificationEmailCommand
        {
            RegistrationId = notification.RegistrationId
        }, cancellationToken);
    }
}

/// <summary>
/// Handler for EmailVerificationRequestedEvent - sends the actual email
/// </summary>
public class EmailVerificationRequestedHandler : INotificationHandler<EmailVerificationRequestedEvent>
{
    private readonly IEmailService _emailService;

    public EmailVerificationRequestedHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(EmailVerificationRequestedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendVerificationEmailAsync(
            notification.Email.Value,
            "User", // We could enhance this to include the actual name
            notification.Token.Token,
            cancellationToken);
    }
}

/// <summary>
/// Handler for EmailVerifiedEvent - creates user in Keycloak
/// </summary>
public class EmailVerifiedHandler : INotificationHandler<EmailVerifiedEvent>
{
    private readonly IKeycloakService _keycloakService;
    private readonly IMediator _mediator;
    private readonly IUserRegistrationRepository _userRegistrationRepository;

    public EmailVerifiedHandler(
        IKeycloakService keycloakService,
        IMediator mediator,
        IUserRegistrationRepository userRegistrationRepository)
    {
        _keycloakService = keycloakService;
        _mediator = mediator;
        _userRegistrationRepository = userRegistrationRepository;
    }

    public async Task Handle(EmailVerifiedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get registration to get user details
            var registration = await _userRegistrationRepository.GetByIdAsync(notification.RegistrationId, cancellationToken);
            if (registration == null)
            {
                throw new InvalidOperationException($"Registration {notification.RegistrationId} not found");
            }

            // Create user in Keycloak
            var externalUserId = await _keycloakService.CreateUserAsync(
                registration.Email.Value,
                registration.Name.FirstName,
                registration.Name.LastName,
                cancellationToken);

            // Complete registration
            await _mediator.Send(new CompleteRegistrationCommand
            {
                RegistrationId = notification.RegistrationId,
                ExternalUserId = externalUserId
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            // If Keycloak user creation fails, we should handle this gracefully
            // For now, we'll let the exception bubble up, but in a real system
            // you might want to implement retry logic or alternative flows
            throw new InvalidOperationException($"Failed to create external user for registration {notification.RegistrationId}", ex);
        }
    }
}

/// <summary>
/// Handler for UserRegistrationCompletedEvent - sends welcome email
/// </summary>
public class UserRegistrationCompletedHandler : INotificationHandler<UserRegistrationCompletedEvent>
{
    private readonly IEmailService _emailService;

    public UserRegistrationCompletedHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(
            notification.Email.Value,
            notification.Name.FirstName,
            cancellationToken);
    }
}

/// <summary>
/// Handler for UserRegistrationCancelledEvent - sends cancellation email
/// </summary>
public class UserRegistrationCancelledHandler : INotificationHandler<UserRegistrationCancelledEvent>
{
    private readonly IEmailService _emailService;

    public UserRegistrationCancelledHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(UserRegistrationCancelledEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendRegistrationCancelledEmailAsync(
            notification.Email.Value,
            "User", // We don't have the name in the event, could be improved
            notification.Reason,
            cancellationToken);
    }
}
