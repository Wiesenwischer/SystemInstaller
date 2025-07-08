using Microsoft.EntityFrameworkCore;
using SystemInstaller.Web.Data;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Application.DTOs;

namespace SystemInstaller.Web.Services
{
    public class TenantService
    {
        private readonly SystemInstallerDbContext _context;
        private readonly ITenantApplicationService _tenantApplicationService;
        
        public TenantService(SystemInstallerDbContext context, ITenantApplicationService tenantApplicationService)
        {
            _context = context;
            _tenantApplicationService = tenantApplicationService;
        }
        
        // Tenant Management (using new architecture where possible)
        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            // Gradually migrate to new architecture
            var tenantDtos = await _tenantApplicationService.GetAllTenantsAsync();
            return tenantDtos.Select(MapToLegacyTenant).ToList();
        }
        
        public async Task<Tenant?> GetTenantByIdAsync(Guid id)
        {
            var tenantDto = await _tenantApplicationService.GetTenantByIdAsync(id);
            return tenantDto != null ? MapToLegacyTenant(tenantDto) : null;
        }
        
        private static Tenant MapToLegacyTenant(TenantDto dto)
        {
            return new Tenant
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
        
        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }
        
        public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }
        
        public async Task<bool> DeleteTenantAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return false;
            
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            return true;
        }
        
        // User Management
        public async Task<List<TenantUser>> GetTenantUsersAsync(Guid tenantId)
        {
            return await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId)
                .OrderBy(tu => tu.Email)
                .ToListAsync();
        }
        
        public async Task<TenantUser?> GetTenantUserAsync(Guid tenantId, string userId)
        {
            return await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);
        }
        
        public async Task<TenantUser> AddUserToTenantAsync(TenantUser tenantUser)
        {
            _context.TenantUsers.Add(tenantUser);
            await _context.SaveChangesAsync();
            return tenantUser;
        }
        
        public async Task<bool> RemoveUserFromTenantAsync(Guid tenantId, string userId)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);
            
            if (tenantUser == null) return false;
            
            _context.TenantUsers.Remove(tenantUser);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> UpdateUserRoleAsync(Guid tenantId, string userId, string role)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);
            
            if (tenantUser == null) return false;
            
            tenantUser.Role = role;
            await _context.SaveChangesAsync();
            return true;
        }
        
        // Helper Methods
        public async Task<bool> IsTenantNameAvailableAsync(string name, Guid? excludeId = null)
        {
            return !await _context.Tenants
                .AnyAsync(t => t.Name == name && (excludeId == null || t.Id != excludeId));
        }
        
        public async Task<List<Tenant>> GetUserTenantsAsync(string userId)
        {
            return await _context.TenantUsers
                .Where(tu => tu.UserId == userId && tu.IsActive)
                .Select(tu => tu.Tenant)
                .Distinct()
                .ToListAsync();
        }
        
        public async Task<bool> IsUserAdminOfTenantAsync(string userId, Guid tenantId)
        {
            return await _context.TenantUsers
                .AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && 
                               tu.Role == "Admin" && tu.IsActive);
        }
        
        public async Task<bool> IsUserMemberOfTenantAsync(string userId, Guid tenantId)
        {
            return await _context.TenantUsers
                .AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && tu.IsActive);
        }
    }
}
