using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.Repositories;
using SystemInstaller.Infrastructure.Data;

namespace SystemInstaller.Infrastructure.Repositories;

public class UserInvitationRepository : IUserInvitationRepository
{
    private readonly SystemInstallerDbContext _context;

    public UserInvitationRepository(SystemInstallerDbContext context)
    {
        _context = context;
    }

    public async Task<UserInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .FirstOrDefaultAsync(ui => ui.Id == id);
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .FirstOrDefaultAsync(ui => ui.InvitationToken == token);
    }

    public async Task<IEnumerable<UserInvitation>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .Where(ui => ui.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingAsync(Guid tenantId)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .Where(ui => ui.TenantId == tenantId && !ui.IsUsed && ui.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<UserInvitation> CreateAsync(UserInvitation invitation)
    {
        _context.UserInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task UpdateAsync(UserInvitation invitation)
    {
        _context.UserInvitations.Update(invitation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var invitation = await _context.UserInvitations.FindAsync(id);
        if (invitation != null)
        {
            _context.UserInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(UserInvitation invitation)
    {
        _context.UserInvitations.Remove(invitation);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.UserInvitations.AnyAsync(ui => ui.Id == id);
    }

    public async Task<bool> ExistsByEmailAndTenantIdAsync(string email, Guid tenantId)
    {
        return await _context.UserInvitations.AnyAsync(ui => 
            ui.Email.Value == email && ui.TenantId == tenantId && !ui.IsUsed);
    }

    public async Task<UserInvitation?> GetByTenantAndEmailAsync(Guid tenantId, string email)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .FirstOrDefaultAsync(ui => ui.TenantId == tenantId && ui.Email.Value == email && !ui.IsUsed);
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingByTenantAsync(Guid tenantId)
    {
        return await _context.UserInvitations
            .Include(ui => ui.Tenant)
            .Where(ui => ui.TenantId == tenantId && !ui.IsUsed && ui.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(UserInvitation invitation)
    {
        _context.UserInvitations.Add(invitation);
        await _context.SaveChangesAsync();
    }
}
