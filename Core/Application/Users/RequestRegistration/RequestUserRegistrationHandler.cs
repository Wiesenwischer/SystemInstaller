using FluentValidation;
using MediatR;
using SystemInstaller.Domain.Users;
using SystemInstaller.Domain.Tenants; // For Email and PersonName
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Application.Users.RequestRegistration;

public class RequestUserRegistrationCommand : IRequest<RequestUserRegistrationResult>
{
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool EmailNotifications { get; set; } = true;
    public bool InstallationUpdates { get; set; } = true;
    public bool SystemMaintenance { get; set; } = true;
}

public class RequestUserRegistrationResult
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = default!;
}

public class RequestUserRegistrationValidator : AbstractValidator<RequestUserRegistrationCommand>
{
    public RequestUserRegistrationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320)
            .WithMessage("A valid email address is required");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("First name is required and must be less than 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Last name is required and must be less than 100 characters");
    }
}

public class RequestUserRegistrationHandler : IRequestHandler<RequestUserRegistrationCommand, RequestUserRegistrationResult>
{
    private readonly IUserRegistrationRepository _userRegistrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestUserRegistrationHandler(
        IUserRegistrationRepository userRegistrationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRegistrationRepository = userRegistrationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RequestUserRegistrationResult> Handle(RequestUserRegistrationCommand request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);
        var name = new PersonName(request.FirstName, request.LastName);
        
        // Check if user already exists
        if (await _userRepository.ExistsAsync(email, cancellationToken))
        {
            throw new BusinessRuleViolationException($"User with email '{request.Email}' already exists");
        }
        
        // Check if there's already a pending registration for this email
        var existingRegistration = await _userRegistrationRepository.GetByEmailAsync(email, cancellationToken);
        if (existingRegistration != null)
        {
            if (existingRegistration.Status == RegistrationStatus.Pending || 
                existingRegistration.Status == RegistrationStatus.EmailSent)
            {
                throw new BusinessRuleViolationException($"Registration already pending for email '{request.Email}'");
            }
            
            if (existingRegistration.Status == RegistrationStatus.Completed)
            {
                throw new BusinessRuleViolationException($"User with email '{request.Email}' is already registered");
            }
        }
        
        // Create notification preferences
        var notificationPreferences = new NotificationPreferences(
            request.EmailNotifications,
            request.InstallationUpdates,
            request.SystemMaintenance);
        
        // Create new user registration
        var registration = new UserRegistration(email, name, notificationPreferences);
        
        // Save to repository
        await _userRegistrationRepository.AddAsync(registration, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Return result
        return new RequestUserRegistrationResult
        {
            RegistrationId = registration.Id,
            Email = registration.Email.Value,
            FirstName = registration.Name.FirstName,
            LastName = registration.Name.LastName,
            CreatedAt = registration.CreatedAt,
            Message = "Registration request received. Please check your email for verification instructions."
        };
    }
}
