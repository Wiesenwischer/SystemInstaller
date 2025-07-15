using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants;

/// <summary>
/// Repository interface for Tenant aggregate
/// </summary>
public interface ITenantRepository : IRepository<Tenant, TenantId>
{
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserInvitation
/// </summary>
public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<UserInvitation>> GetPendingInvitationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(UserInvitation invitation, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);
}
