using FluentValidation;

namespace SystemInstaller.Application.Tenants.CreateTenant;

public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required")
            .MaximumLength(100)
            .WithMessage("Tenant name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .WithMessage("Contact email is required")
            .EmailAddress()
            .WithMessage("Contact email must be a valid email address");
    }
}
