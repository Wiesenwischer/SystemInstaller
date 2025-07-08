using Microsoft.EntityFrameworkCore;
using SystemInstaller.Web.Data;

namespace SystemInstaller.Web.Services
{
    public class TenantService
    {
        private readonly SystemInstallerDbContext _context;
        
        public TenantService(SystemInstallerDbContext context)
        {
            _context = context;
        }
        
        // Tenant Management
        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants
                .Include(t => t.TenantUsers)
                .Include(t => t.Environments)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        
        public async Task<Tenant?> GetTenantByIdAsync(Guid id)
        {
            return await _context.Tenants
                .Include(t => t.TenantUsers)
                .Include(t => t.Environments)
                .FirstOrDefaultAsync(t => t.Id == id);
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
