using FluentValidation;
using MediatR;
using SystemInstaller.Domain.Users;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Application.Users.VerifyEmail;

public class VerifyEmailCommand : IRequest<VerifyEmailResult>
{
    public string Token { get; set; } = default!;
}

public class VerifyEmailResult
{
    public UserRegistrationId RegistrationId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime VerifiedAt { get; set; }
    public string Message { get; set; } = default!;
}

public class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Verification token is required");
    }
}

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IUserRegistrationRepository _userRegistrationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(
        IUserRegistrationRepository userRegistrationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRegistrationRepository = userRegistrationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var registration = await _userRegistrationRepository.GetByVerificationTokenAsync(request.Token, cancellationToken);
        if (registration == null)
        {
            throw new BusinessRuleViolationException("Invalid or expired verification token");
        }

        // Verify the email
        registration.VerifyEmail(request.Token);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResult
        {
            RegistrationId = registration.Id,
            Email = registration.Email.Value,
            VerifiedAt = registration.VerifiedAt!.Value,
            Message = "Email verified successfully. Creating your account..."
        };
    }
}
