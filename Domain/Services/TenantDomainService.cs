using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Domain.Services;

public interface ITenantDomainService
{
    Task<Tenant> CreateTenantAsync(string name, Email contactEmail, string? description = null);
    Task<bool> CanDeleteTenantAsync(Guid tenantId);
    Task<TenantUser> InviteUserAsync(Guid tenantId, Email email, PersonName name, UserRole role, string? invitedByUserId = null);
    Task<TenantUser> AcceptInvitationAsync(string token, string userId);
}

public class TenantDomainService : ITenantDomainService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUserRepository _tenantUserRepository;
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IInstallationEnvironmentRepository _environmentRepository;

    public TenantDomainService(
        ITenantRepository tenantRepository,
        ITenantUserRepository tenantUserRepository,
        IUserInvitationRepository invitationRepository,
        IInstallationEnvironmentRepository environmentRepository)
    {
        _tenantRepository = tenantRepository;
        _tenantUserRepository = tenantUserRepository;
        _invitationRepository = invitationRepository;
        _environmentRepository = environmentRepository;
    }

    public async Task<Tenant> CreateTenantAsync(string name, Email contactEmail, string? description = null)
    {
        // Check if tenant with same name already exists
        if (await _tenantRepository.ExistsByNameAsync(name))
            throw new InvalidOperationException($"Tenant with name '{name}' already exists");

        var tenant = new Tenant(name, contactEmail, description);
        return await _tenantRepository.CreateAsync(tenant);
    }

    public async Task<bool> CanDeleteTenantAsync(Guid tenantId)
    {
        // Check if tenant has any environments
        var environments = await _environmentRepository.GetByTenantIdAsync(tenantId);
        if (environments.Any())
            return false;

        // Check if tenant has any users
        var users = await _tenantUserRepository.GetByTenantIdAsync(tenantId);
        if (users.Any())
            return false;

        return true;
    }

    public async Task<TenantUser> InviteUserAsync(Guid tenantId, Email email, PersonName name, UserRole role, string? invitedByUserId = null)
    {
        // Check if tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        if (!tenant.IsActive)
            throw new InvalidOperationException("Cannot invite users to inactive tenant");

        // Check if user is already a member
        if (await _invitationRepository.ExistsByEmailAndTenantIdAsync(email, tenantId))
            throw new InvalidOperationException("User is already invited to this tenant");

        // Create invitation
        var invitation = new UserInvitation(tenantId, email, name, role, invitedByUserId);
        await _invitationRepository.CreateAsync(invitation);

        // Here you would typically send an email notification
        // This could be handled by an application service or event handler

        return new TenantUser(tenantId, string.Empty, email, name, role); // Placeholder until invitation is accepted
    }

    public async Task<TenantUser> AcceptInvitationAsync(string token, string userId)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        if (invitation == null)
            throw new InvalidOperationException("Invalid invitation token");

        if (!invitation.IsValid)
            throw new InvalidOperationException("Invitation is expired or already used");

        // Check if user is already a member of the tenant
        if (await _tenantUserRepository.ExistsByUserIdAndTenantIdAsync(userId, invitation.TenantId))
            throw new InvalidOperationException("User is already a member of this tenant");

        // Mark invitation as used
        invitation.Use();
        await _invitationRepository.UpdateAsync(invitation);

        // Create tenant user
        var tenantUser = new TenantUser(invitation.TenantId, userId, invitation.Email, invitation.Name, invitation.Role);
        return await _tenantUserRepository.CreateAsync(tenantUser);
    }
}
