using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly SystemInstallerDbContext _context;

    public TenantRepository(SystemInstallerDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants
            .Include(t => t.TenantUsers)
            .Include(t => t.Environments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetByNameAsync(string name)
    {
        return await _context.Tenants
            .Include(t => t.TenantUsers)
            .Include(t => t.Environments)
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants
            .Include(t => t.TenantUsers)
            .Include(t => t.Environments)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        return await _context.Tenants
            .Include(t => t.TenantUsers)
            .Include(t => t.Environments)
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Tenants.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Tenants.AnyAsync(t => t.Name == name);
    }
}
