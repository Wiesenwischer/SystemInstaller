using SystemInstaller.Application.DTOs;

namespace SystemInstaller.Application.Interfaces;

public interface ITenantApplicationService
{
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
    Task<IEnumerable<TenantDto>> GetActiveTenantsAsync();
    Task<TenantDto?> GetTenantByIdAsync(Guid id);
    Task<TenantDetailsDto?> GetTenantDetailsAsync(Guid id);
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateTenantAsync(Guid id, UpdateTenantDto dto);
    Task DeleteTenantAsync(Guid id);
    Task<TenantDto> ActivateTenantAsync(Guid id);
    Task<TenantDto> DeactivateTenantAsync(Guid id);
    Task RemoveUserFromTenantAsync(Guid tenantId, string userId);
    Task UpdateUserRoleAsync(Guid tenantId, string userId, string role);
}
