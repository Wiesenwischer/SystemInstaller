using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Application.UseCases;

public class UserInvitationApplicationService : IUserInvitationApplicationService
{
    private readonly IUserInvitationRepository _userInvitationRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUserRepository _tenantUserRepository;
    private readonly IEmailService _emailService;

    public UserInvitationApplicationService(
        IUserInvitationRepository userInvitationRepository,
        ITenantRepository tenantRepository,
        ITenantUserRepository tenantUserRepository,
        IEmailService emailService)
    {
        _userInvitationRepository = userInvitationRepository;
        _tenantRepository = tenantRepository;
        _tenantUserRepository = tenantUserRepository;
        _emailService = emailService;
    }

    public async Task<UserInvitationResultDto> CreateInvitationAsync(CreateUserInvitationDto dto)
    {
        // Check if user is already invited or member
        var existingInvitation = await _userInvitationRepository.GetByTenantAndEmailAsync(dto.TenantId, dto.Email);
        if (existingInvitation != null)
        {
            throw new InvalidOperationException("Eine Einladung für diese E-Mail-Adresse existiert bereits.");
        }

        var existingUser = await _tenantUserRepository.GetByTenantAndEmailAsync(dto.TenantId, dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Benutzer ist bereits Mitglied dieses Tenants.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(dto.TenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        var email = new Email(dto.Email);
        var name = new PersonName(dto.FirstName, dto.LastName);
        var role = new UserRole(dto.Role);

        var invitation = new UserInvitation(dto.TenantId, email, name, role, dto.InvitedByUserId);
        
        await _userInvitationRepository.AddAsync(invitation);

        // Send invitation email
        await _emailService.SendInvitationEmailAsync(dto.Email, tenant.Name, invitation.InvitationToken);

        return MapToDto(invitation, tenant.Name);
    }

    public async Task<UserInvitationResultDto?> GetInvitationByTokenAsync(string token)
    {
        var invitation = await _userInvitationRepository.GetByTokenAsync(token);
        if (invitation == null) return null;

        var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId);
        return MapToDto(invitation, tenant?.Name);
    }

    public async Task<bool> AcceptInvitationAsync(AcceptInvitationDto dto)
    {
        var invitation = await _userInvitationRepository.GetByTokenAsync(dto.Token);
        if (invitation == null || invitation.IsUsed || invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        // Check if user is already member
        var existingUser = await _tenantUserRepository.GetByTenantAndUserIdAsync(invitation.TenantId, dto.UserId);
        if (existingUser != null)
        {
            // Mark invitation as used
            invitation.MarkAsUsed();
            await _userInvitationRepository.UpdateAsync(invitation);
            return true;
        }

        // Add user to tenant
        var tenantUser = new TenantUser(invitation.TenantId, dto.UserId, invitation.Email, invitation.Name, invitation.Role);
        await _tenantUserRepository.AddAsync(tenantUser);

        // Mark invitation as used
        invitation.MarkAsUsed();
        await _userInvitationRepository.UpdateAsync(invitation);

        return true;
    }

    public async Task<List<UserInvitationResultDto>> GetPendingInvitationsAsync(Guid tenantId)
    {
        var invitations = await _userInvitationRepository.GetPendingByTenantAsync(tenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        
        return invitations.Select(i => MapToDto(i, tenant?.Name)).ToList();
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId)
    {
        var invitation = await _userInvitationRepository.GetByIdAsync(invitationId);
        if (invitation == null || invitation.IsUsed)
        {
            return false;
        }

        await _userInvitationRepository.DeleteAsync(invitation);
        return true;
    }

    public async Task<bool> ResendInvitationAsync(Guid invitationId)
    {
        var invitation = await _userInvitationRepository.GetByIdAsync(invitationId);
        if (invitation == null || invitation.IsUsed || invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId);
        if (tenant == null) return false;

        // Extend expiration
        invitation.ExtendExpiration(7);
        await _userInvitationRepository.UpdateAsync(invitation);

        // Resend email
        await _emailService.SendInvitationEmailAsync(invitation.Email.Value, tenant.Name, invitation.InvitationToken);
        
        return true;
    }

    private static UserInvitationResultDto MapToDto(UserInvitation invitation, string? tenantName = null)
    {
        return new UserInvitationResultDto
        {
            Id = invitation.Id,
            TenantId = invitation.TenantId,
            Email = invitation.Email.Value,
            FirstName = invitation.Name.FirstName,
            LastName = invitation.Name.LastName,
            Role = invitation.Role.Value,
            InvitationToken = invitation.InvitationToken,
            IsUsed = invitation.IsUsed,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            UsedAt = invitation.UsedAt,
            InvitedByUserId = invitation.InvitedByUserId,
            TenantName = tenantName
        };
    }
}
