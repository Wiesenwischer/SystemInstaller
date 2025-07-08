using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Infrastructure.Adapters;

/// <summary>
/// Legacy adapter for InvitationService to gradually migrate to new architecture
/// </summary>
public class LegacyInvitationServiceAdapter
{
    private readonly IUserInvitationApplicationService _userInvitationService;

    public LegacyInvitationServiceAdapter(IUserInvitationApplicationService userInvitationService)
    {
        _userInvitationService = userInvitationService;
    }

    public async Task<UserInvitation> CreateInvitationAsync(
        Guid tenantId, 
        string email, 
        string firstName, 
        string lastName, 
        string role, 
        string invitedByUserId)
    {
        var dto = new CreateUserInvitationDto
        {
            TenantId = tenantId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            InvitedByUserId = invitedByUserId
        };

        var result = await _userInvitationService.CreateInvitationAsync(dto);
        
        // Map back to legacy model for compatibility
        return new UserInvitation
        {
            Id = result.Id,
            TenantId = result.TenantId,
            Email = result.Email,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Role = result.Role,
            InvitationToken = result.InvitationToken,
            IsUsed = result.IsUsed,
            CreatedAt = result.CreatedAt,
            ExpiresAt = result.ExpiresAt,
            UsedAt = result.UsedAt,
            InvitedByUserId = result.InvitedByUserId
        };
    }

    public async Task<UserInvitation?> GetInvitationByTokenAsync(string token)
    {
        var result = await _userInvitationService.GetInvitationByTokenAsync(token);
        if (result == null) return null;

        return new UserInvitation
        {
            Id = result.Id,
            TenantId = result.TenantId,
            Email = result.Email,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Role = result.Role,
            InvitationToken = result.InvitationToken,
            IsUsed = result.IsUsed,
            CreatedAt = result.CreatedAt,
            ExpiresAt = result.ExpiresAt,
            UsedAt = result.UsedAt,
            InvitedByUserId = result.InvitedByUserId,
            Tenant = result.TenantName != null ? new Tenant { Name = result.TenantName } : null!
        };
    }

    public async Task<bool> AcceptInvitationAsync(string token, string userId)
    {
        var dto = new AcceptInvitationDto(token, userId);

        return await _userInvitationService.AcceptInvitationAsync(dto);
    }

    public async Task<List<UserInvitation>> GetPendingInvitationsAsync(Guid tenantId)
    {
        var results = await _userInvitationService.GetPendingInvitationsAsync(tenantId);
        
        return results.Select(r => new UserInvitation
        {
            Id = r.Id,
            TenantId = r.TenantId,
            Email = r.Email,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Role = r.Role,
            InvitationToken = r.InvitationToken,
            IsUsed = r.IsUsed,
            CreatedAt = r.CreatedAt,
            ExpiresAt = r.ExpiresAt,
            UsedAt = r.UsedAt,
            InvitedByUserId = r.InvitedByUserId
        }).ToList();
    }

    public async Task<bool> CancelInvitationAsync(Guid invitationId)
    {
        return await _userInvitationService.CancelInvitationAsync(invitationId);
    }

    public async Task<bool> ResendInvitationAsync(Guid invitationId)
    {
        return await _userInvitationService.ResendInvitationAsync(invitationId);
    }
}
