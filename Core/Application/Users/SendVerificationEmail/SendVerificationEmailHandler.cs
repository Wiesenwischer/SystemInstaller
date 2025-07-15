using FluentValidation;
using MediatR;
using SystemInstaller.Domain.Users;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Application.Users.SendVerificationEmail;

public class SendVerificationEmailCommand : IRequest<SendVerificationEmailResult>
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public TimeSpan? TokenValidFor { get; set; } // Optional custom token validity
}

public class SendVerificationEmailResult
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime EmailSentAt { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public string Message { get; set; } = default!;
}

public class SendVerificationEmailValidator : AbstractValidator<SendVerificationEmailCommand>
{
    public SendVerificationEmailValidator()
    {
        RuleFor(x => x.RegistrationId)
            .NotEmpty()
            .WithMessage("Registration ID is required");

        RuleFor(x => x.TokenValidFor)
            .Must(span => span == null || span > TimeSpan.Zero)
            .WithMessage("Token validity period must be positive");
    }
}

public class SendVerificationEmailHandler : IRequestHandler<SendVerificationEmailCommand, SendVerificationEmailResult>
{
    private readonly IUserRegistrationRepository _userRegistrationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendVerificationEmailHandler(
        IUserRegistrationRepository userRegistrationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRegistrationRepository = userRegistrationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SendVerificationEmailResult> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var registration = await _userRegistrationRepository.GetByIdAsync(request.RegistrationId, cancellationToken);
        if (registration == null)
        {
            throw new EntityNotFoundException(nameof(UserRegistration), request.RegistrationId);
        }

        // Send verification email (this will generate a new token)
        registration.SendVerificationEmail(request.TokenValidFor);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SendVerificationEmailResult
        {
            RegistrationId = registration.Id,
            Email = registration.Email.Value,
            EmailSentAt = registration.EmailSentAt!.Value,
            TokenExpiresAt = registration.VerificationToken!.ExpiresAt,
            Message = "Verification email sent successfully"
        };
    }
}
