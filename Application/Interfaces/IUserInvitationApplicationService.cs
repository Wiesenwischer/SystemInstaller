using SystemInstaller.Application.DTOs;

namespace SystemInstaller.Application.Interfaces;

public interface IUserInvitationApplicationService
{
    Task<IEnumerable<UserInvitationDto>> GetInvitationsByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<UserInvitationDto>> GetPendingInvitationsAsync(Guid tenantId);
    Task<UserInvitationDto?> GetInvitationByTokenAsync(string token);
    Task<UserInvitationDto> CreateInvitationAsync(CreateInvitationDto dto);
    Task<TenantUserDto> AcceptInvitationAsync(AcceptInvitationDto dto);
    Task DeleteInvitationAsync(Guid id);
    Task<UserInvitationDto> ExtendInvitationAsync(Guid id, int days);
}
