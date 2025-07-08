using SystemInstaller.Application.DTOs;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Domain.Services;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Application.UseCases;

public class TenantApplicationService : ITenantApplicationService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUserRepository _tenantUserRepository;
    private readonly IInstallationEnvironmentRepository _environmentRepository;
    private readonly ITenantDomainService _tenantDomainService;

    public TenantApplicationService(
        ITenantRepository tenantRepository,
        ITenantUserRepository tenantUserRepository,
        IInstallationEnvironmentRepository environmentRepository,
        ITenantDomainService tenantDomainService)
    {
        _tenantRepository = tenantRepository;
        _tenantUserRepository = tenantUserRepository;
        _environmentRepository = environmentRepository;
        _tenantDomainService = tenantDomainService;
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return await MapToTenantDtosAsync(tenants);
    }

    public async Task<IEnumerable<TenantDto>> GetActiveTenantsAsync()
    {
        var tenants = await _tenantRepository.GetActiveAsync();
        return await MapToTenantDtosAsync(tenants);
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            return null;

        return await MapToTenantDtoAsync(tenant);
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
    {
        var tenant = await _tenantDomainService.CreateTenantAsync(
            dto.Name,
            new Email(dto.ContactEmail),
            dto.Description);

        return await MapToTenantDtoAsync(tenant);
    }

    public async Task<TenantDto> UpdateTenantAsync(Guid id, UpdateTenantDto dto)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        tenant.UpdateName(dto.Name);
        tenant.UpdateDescription(dto.Description);
        tenant.UpdateContactEmail(new Email(dto.ContactEmail));

        await _tenantRepository.UpdateAsync(tenant);

        return await MapToTenantDtoAsync(tenant);
    }

    public async Task DeleteTenantAsync(Guid id)
    {
        if (!await _tenantDomainService.CanDeleteTenantAsync(id))
            throw new InvalidOperationException("Cannot delete tenant with existing users or environments");

        await _tenantRepository.DeleteAsync(id);
    }

    public async Task<TenantDto> ActivateTenantAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        tenant.Activate();
        await _tenantRepository.UpdateAsync(tenant);

        return await MapToTenantDtoAsync(tenant);
    }

    public async Task<TenantDto> DeactivateTenantAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        tenant.Deactivate();
        await _tenantRepository.UpdateAsync(tenant);

        return await MapToTenantDtoAsync(tenant);
    }

    public async Task<TenantDetailsDto?> GetTenantDetailsAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            return null;

        var users = await _tenantUserRepository.GetByTenantIdAsync(tenant.Id);
        var environments = await _environmentRepository.GetByTenantIdAsync(tenant.Id);

        var userDtos = users.Select(u => new TenantUserDto(
            u.Id,
            u.TenantId,
            u.UserId,
            u.Email.Value,
            u.Name.FirstName,
            u.Name.LastName,
            $"{u.Name.FirstName} {u.Name.LastName}",
            u.Role.Value,
            u.IsActive,
            u.CreatedAt,
            u.LastLoginAt
        )).ToList();

        var environmentDtos = environments.Select(e => new TenantEnvironmentDto(
            e.Id,
            e.Name,
            e.Description,
            e.CreatedAt
        )).ToList();

        return new TenantDetailsDto(
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.ContactEmail.Value,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.UpdatedAt,
            userDtos,
            environmentDtos
        );
    }

    public async Task RemoveUserFromTenantAsync(Guid tenantId, string userId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
            throw new InvalidOperationException("Tenant not found");

        tenant.RemoveUser(userId);
        await _tenantRepository.UpdateAsync(tenant);
    }

    public async Task UpdateUserRoleAsync(Guid tenantId, string userId, string role)
    {
        var tenantUser = await _tenantUserRepository.GetByTenantAndUserIdAsync(tenantId, userId);
        if (tenantUser == null)
            throw new InvalidOperationException("User not found in tenant");

        tenantUser.UpdateRole(new UserRole(role));
        await _tenantUserRepository.UpdateAsync(tenantUser);
    }

    private async Task<IEnumerable<TenantDto>> MapToTenantDtosAsync(IEnumerable<Tenant> tenants)
    {
        var result = new List<TenantDto>();
        foreach (var tenant in tenants)
        {
            result.Add(await MapToTenantDtoAsync(tenant));
        }
        return result;
    }

    private async Task<TenantDto> MapToTenantDtoAsync(Tenant tenant)
    {
        var users = await _tenantUserRepository.GetByTenantIdAsync(tenant.Id);
        var environments = await _environmentRepository.GetByTenantIdAsync(tenant.Id);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.ContactEmail,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.UpdatedAt,
            users.Count(),
            environments.Count()
        );
    }
}
