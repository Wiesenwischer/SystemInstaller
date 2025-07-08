using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure.Repositories;

public class InstallationTaskRepository : IInstallationTaskRepository
{
    private readonly SystemInstallerDbContext _context;

    public InstallationTaskRepository(SystemInstallerDbContext context)
    {
        _context = context;
    }

    public async Task<InstallationTask?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.Environment)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<InstallationTask>> GetByEnvironmentIdAsync(Guid environmentId)
    {
        return await _context.Tasks
            .Include(t => t.Environment)
            .Where(t => t.EnvironmentId == environmentId)
            .ToListAsync();
    }

    public async Task<InstallationTask> CreateAsync(InstallationTask task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(InstallationTask task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<InstallationTask>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.Environment)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<InstallationTask>> GetByStatusAsync(Domain.ValueObjects.InstallationStatus status)
    {
        return await _context.Tasks
            .Include(t => t.Environment)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task AddAsync(InstallationTask task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }
}
