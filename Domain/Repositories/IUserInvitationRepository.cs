using SystemInstaller.Domain.Entities;

namespace SystemInstaller.Domain.Repositories;

public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByIdAsync(Guid id);
    Task<UserInvitation?> GetByTokenAsync(string token);
    Task<UserInvitation?> GetByTenantAndEmailAsync(Guid tenantId, string email);
    Task<IEnumerable<UserInvitation>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<UserInvitation>> GetPendingAsync(Guid tenantId);
    Task<IEnumerable<UserInvitation>> GetPendingByTenantAsync(Guid tenantId);
    Task<UserInvitation> CreateAsync(UserInvitation invitation);
    Task AddAsync(UserInvitation invitation);
    Task UpdateAsync(UserInvitation invitation);
    Task DeleteAsync(UserInvitation invitation);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByEmailAndTenantIdAsync(string email, Guid tenantId);
}
