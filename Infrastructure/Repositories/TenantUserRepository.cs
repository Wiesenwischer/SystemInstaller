using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure.Repositories;

public class TenantUserRepository : ITenantUserRepository
{
    private readonly SystemInstallerDbContext _context;

    public TenantUserRepository(SystemInstallerDbContext context)
    {
        _context = context;
    }

    public async Task<TenantUser?> GetByIdAsync(Guid id)
    {
        return await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.Id == id);
    }

    public async Task<TenantUser?> GetByUserIdAsync(string userId)
    {
        return await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.UserId == userId);
    }

    public async Task<IEnumerable<TenantUser>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .Where(tu => tu.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<TenantUser> CreateAsync(TenantUser tenantUser)
    {
        _context.TenantUsers.Add(tenantUser);
        await _context.SaveChangesAsync();
        return tenantUser;
    }

    public async Task UpdateAsync(TenantUser tenantUser)
    {
        _context.TenantUsers.Update(tenantUser);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenantUser = await _context.TenantUsers.FindAsync(id);
        if (tenantUser != null)
        {
            _context.TenantUsers.Remove(tenantUser);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.TenantUsers.AnyAsync(tu => tu.Id == id);
    }

    public async Task<bool> ExistsByUserIdAndTenantIdAsync(string userId, Guid tenantId)
    {
        return await _context.TenantUsers.AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);
    }

    public async Task<TenantUser?> GetByTenantAndUserIdAsync(Guid tenantId, string userId)
    {
        return await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);
    }

    public async Task<TenantUser?> GetByTenantAndEmailAsync(Guid tenantId, string email)
    {
        return await _context.TenantUsers
            .Include(tu => tu.Tenant)
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.Email.Value == email);
    }

    public async Task AddAsync(TenantUser tenantUser)
    {
        _context.TenantUsers.Add(tenantUser);
        await _context.SaveChangesAsync();
    }
}
