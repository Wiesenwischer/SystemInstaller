using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.TenantManagement;

/// <summary>
/// User role within a tenant
/// </summary>
public enum UserRole
{
    Admin,
    User,
    ReadOnly
}

/// <summary>
/// Represents a user within a tenant context
/// </summary>
public class TenantUser : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = default!; // From Identity Provider
    public Email Email { get; private set; } = default!;
    public PersonName Name { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;

    private TenantUser() { } // For EF Core

    internal TenantUser(Guid tenantId, string userId, Email email, PersonName name, UserRole role)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        Email = email;
        Name = name;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole newRole)
    {
        if (Role != newRole)
        {
            Role = newRole;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
