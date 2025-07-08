using SystemInstaller.Domain.Entities;

namespace SystemInstaller.Domain.Repositories;

public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByIdAsync(Guid id);
    Task<UserInvitation?> GetByTokenAsync(string token);
    Task<IEnumerable<UserInvitation>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<UserInvitation>> GetPendingAsync(Guid tenantId);
    Task<UserInvitation> CreateAsync(UserInvitation invitation);
    Task UpdateAsync(UserInvitation invitation);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByEmailAndTenantIdAsync(string email, Guid tenantId);
}
