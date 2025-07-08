using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Infrastructure.Adapters;

// Legacy adapter to bridge old services with new architecture
public class LegacyTenantServiceAdapter
{
    private readonly ITenantApplicationService _tenantApplicationService;

    public LegacyTenantServiceAdapter(ITenantApplicationService tenantApplicationService)
    {
        _tenantApplicationService = tenantApplicationService;
    }

    public async Task<IEnumerable<SystemInstaller.Web.Data.Tenant>> GetAllTenantsAsync()
    {
        var tenantDtos = await _tenantApplicationService.GetAllTenantsAsync();
        return tenantDtos.Select(MapToLegacyTenant);
    }

    public async Task<SystemInstaller.Web.Data.Tenant?> GetTenantByIdAsync(Guid id)
    {
        var tenantDto = await _tenantApplicationService.GetTenantByIdAsync(id);
        return tenantDto != null ? MapToLegacyTenant(tenantDto) : null;
    }

    private static SystemInstaller.Web.Data.Tenant MapToLegacyTenant(TenantDto dto)
    {
        return new SystemInstaller.Web.Data.Tenant
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ContactEmail = dto.ContactEmail,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
