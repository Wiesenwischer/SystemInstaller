using SystemInstaller.Application.DTOs;

namespace SystemInstaller.Application.Interfaces;

public interface IUserInvitationApplicationService
{
    Task<UserInvitationResultDto> CreateInvitationAsync(CreateUserInvitationDto dto);
    Task<UserInvitationResultDto?> GetInvitationByTokenAsync(string token);
    Task<bool> AcceptInvitationAsync(AcceptInvitationDto dto);
    Task<List<UserInvitationResultDto>> GetPendingInvitationsAsync(Guid tenantId);
    Task<bool> CancelInvitationAsync(Guid invitationId);
    Task<bool> ResendInvitationAsync(Guid invitationId);
}
