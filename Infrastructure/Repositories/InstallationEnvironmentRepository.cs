using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure.Repositories;

public class InstallationEnvironmentRepository : IInstallationEnvironmentRepository
{
    private readonly SystemInstallerDbContext _context;

    public InstallationEnvironmentRepository(SystemInstallerDbContext context)
    {
        _context = context;
    }

    public async Task<InstallationEnvironment?> GetByIdAsync(Guid id)
    {
        return await _context.Environments
            .Include(e => e.Tenant)
            .Include(e => e.Tasks)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<InstallationEnvironment>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.Environments
            .Include(e => e.Tenant)
            .Include(e => e.Tasks)
            .Where(e => e.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<InstallationEnvironment> CreateAsync(InstallationEnvironment environment)
    {
        _context.Environments.Add(environment);
        await _context.SaveChangesAsync();
        return environment;
    }

    public async Task UpdateAsync(InstallationEnvironment environment)
    {
        _context.Environments.Update(environment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var environment = await _context.Environments.FindAsync(id);
        if (environment != null)
        {
            _context.Environments.Remove(environment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Environments.AnyAsync(e => e.Id == id);
    }
}
