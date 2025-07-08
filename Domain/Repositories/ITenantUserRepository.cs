using SystemInstaller.Domain.Entities;

namespace SystemInstaller.Domain.Repositories;

public interface ITenantUserRepository
{
    Task<TenantUser?> GetByIdAsync(Guid id);
    Task<TenantUser?> GetByUserIdAsync(string userId);
    Task<IEnumerable<TenantUser>> GetByTenantIdAsync(Guid tenantId);
    Task<TenantUser> CreateAsync(TenantUser tenantUser);
    Task UpdateAsync(TenantUser tenantUser);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByUserIdAndTenantIdAsync(string userId, Guid tenantId);
}
