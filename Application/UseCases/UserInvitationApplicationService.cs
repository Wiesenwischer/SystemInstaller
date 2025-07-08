using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.Services;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Application.UseCases;

public class UserInvitationApplicationService : IUserInvitationApplicationService
{
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly ITenantDomainService _tenantDomainService;

    public UserInvitationApplicationService(
        IUserInvitationRepository invitationRepository,
        ITenantDomainService tenantDomainService)
    {
        _invitationRepository = invitationRepository;
        _tenantDomainService = tenantDomainService;
    }

    public async Task<IEnumerable<UserInvitationDto>> GetInvitationsByTenantIdAsync(Guid tenantId)
    {
        var invitations = await _invitationRepository.GetByTenantIdAsync(tenantId);
        return invitations.Select(MapToDto);
    }

    public async Task<IEnumerable<UserInvitationDto>> GetPendingInvitationsAsync(Guid tenantId)
    {
        var invitations = await _invitationRepository.GetPendingAsync(tenantId);
        return invitations.Select(MapToDto);
    }

    public async Task<UserInvitationDto?> GetInvitationByTokenAsync(string token)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token);
        return invitation != null ? MapToDto(invitation) : null;
    }

    public async Task<UserInvitationDto> CreateInvitationAsync(CreateInvitationDto dto)
    {
        var email = new Email(dto.Email);
        var name = new PersonName(dto.FirstName, dto.LastName);
        var role = new UserRole(dto.Role);

        await _tenantDomainService.InviteUserAsync(
            dto.TenantId,
            email,
            name,
            role,
            dto.InvitedByUserId);

        // Get the created invitation
        var invitations = await _invitationRepository.GetByTenantIdAsync(dto.TenantId);
        var invitation = invitations.FirstOrDefault(i => i.Email.Value == dto.Email && !i.IsUsed);
        
        if (invitation == null)
            throw new InvalidOperationException("Failed to create invitation");

        return MapToDto(invitation);
    }

    public async Task<TenantUserDto> AcceptInvitationAsync(AcceptInvitationDto dto)
    {
        var tenantUser = await _tenantDomainService.AcceptInvitationAsync(dto.Token, dto.UserId);

        return new TenantUserDto(
            tenantUser.Id,
            tenantUser.TenantId,
            tenantUser.UserId,
            tenantUser.Email,
            tenantUser.Name.FirstName,
            tenantUser.Name.LastName,
            tenantUser.Name.FullName,
            tenantUser.Role,
            tenantUser.IsActive,
            tenantUser.CreatedAt,
            tenantUser.LastLoginAt
        );
    }

    public async Task DeleteInvitationAsync(Guid id)
    {
        await _invitationRepository.DeleteAsync(id);
    }

    public async Task<UserInvitationDto> ExtendInvitationAsync(Guid id, int days)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id);
        if (invitation == null)
            throw new InvalidOperationException("Invitation not found");

        invitation.ExtendExpiration(days);
        await _invitationRepository.UpdateAsync(invitation);

        return MapToDto(invitation);
    }

    private static UserInvitationDto MapToDto(UserInvitation invitation)
    {
        return new UserInvitationDto(
            invitation.Id,
            invitation.TenantId,
            invitation.Email,
            invitation.Name.FirstName,
            invitation.Name.LastName,
            invitation.Name.FullName,
            invitation.Role,
            invitation.InvitationToken,
            invitation.IsUsed,
            invitation.IsExpired,
            invitation.IsValid,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.UsedAt,
            invitation.InvitedByUserId
        );
    }
}
