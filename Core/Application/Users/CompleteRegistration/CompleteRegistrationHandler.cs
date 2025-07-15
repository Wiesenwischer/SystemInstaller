using FluentValidation;
using MediatR;
using SystemInstaller.Domain.Users;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Application.Users.CompleteRegistration;

public class CompleteRegistrationCommand : IRequest<CompleteRegistrationResult>
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public string ExternalUserId { get; set; } = default!;
}

public class CompleteRegistrationResult
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public UserId UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string ExternalUserId { get; set; } = default!;
    public DateTime CompletedAt { get; set; }
    public string Message { get; set; } = default!;
}

public class CompleteRegistrationValidator : AbstractValidator<CompleteRegistrationCommand>
{
    public CompleteRegistrationValidator()
    {
        RuleFor(x => x.RegistrationId)
            .NotEmpty()
            .WithMessage("Registration ID is required");

        RuleFor(x => x.ExternalUserId)
            .NotEmpty()
            .WithMessage("External user ID is required");
    }
}

public class CompleteRegistrationHandler : IRequestHandler<CompleteRegistrationCommand, CompleteRegistrationResult>
{
    private readonly IUserRegistrationRepository _userRegistrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteRegistrationHandler(
        IUserRegistrationRepository userRegistrationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRegistrationRepository = userRegistrationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompleteRegistrationResult> Handle(CompleteRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _userRegistrationRepository.GetByIdAsync(request.RegistrationId, cancellationToken);
        if (registration == null)
        {
            throw new EntityNotFoundException(nameof(UserRegistration), request.RegistrationId);
        }

        // Check if external user already exists
        var existingUser = await _userRepository.GetByExternalUserIdAsync(request.ExternalUserId, cancellationToken);
        if (existingUser != null)
        {
            throw new BusinessRuleViolationException($"User with external ID '{request.ExternalUserId}' already exists");
        }

        // Mark external user as created
        registration.MarkExternalUserCreated(request.ExternalUserId);

        // Complete the registration (this creates the User entity)
        var user = registration.CompleteRegistration();

        // Add user to repository
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompleteRegistrationResult
        {
            RegistrationId = registration.Id,
            UserId = user.Id,
            Email = user.Email.Value,
            FirstName = user.Name.FirstName,
            LastName = user.Name.LastName,
            ExternalUserId = user.ExternalUserId,
            CompletedAt = registration.CompletedAt!.Value,
            Message = "Registration completed successfully. Welcome to SystemInstaller!"
        };
    }
}
