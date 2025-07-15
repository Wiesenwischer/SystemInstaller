using MediatR;
using SystemInstaller.Core.Domain.TenantManagement;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Application.TenantManagement.InviteUser;

public class InviteUserCommand : IRequest<InviteUserResult>
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public UserRole Role { get; set; }
    public string InvitedByUserId { get; set; } = default!;
}

public class InviteUserResult
{
    public Guid InvitationId { get; set; }
    public string InvitationToken { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}

public class InviteUserHandler : IRequestHandler<InviteUserCommand, InviteUserResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteUserHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InviteUserResult> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        // Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new EntityNotFoundException(nameof(Tenant), request.TenantId);
        }

        // Create invitation
        var email = new Email(request.Email);
        var name = new PersonName(request.FirstName, request.LastName);
        var invitation = tenant.InviteUser(email, name, request.Role, request.InvitedByUserId);

        // Save changes
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return new InviteUserResult
        {
            InvitationId = invitation.Id,
            InvitationToken = invitation.InvitationToken,
            Email = invitation.Email,
            FullName = invitation.Name.FullName,
            ExpiresAt = invitation.ExpiresAt
        };
    }
}
