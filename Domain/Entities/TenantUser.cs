using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Domain.Entities;

public class TenantUser
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = null!; // Keycloak User ID
    public Email Email { get; private set; } = null!;
    public PersonName Name { get; private set; } = null!;
    public UserRole Role { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    // Navigation Properties
    public Tenant Tenant { get; private set; } = null!;
    
    private TenantUser() { } // EF Core
    
    public TenantUser(Guid tenantId, string userId, Email email, PersonName name, UserRole role)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        if (userId.Length > 100)
            throw new ArgumentException("UserId cannot exceed 100 characters", nameof(userId));
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        Email = email;
        Name = name;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void UpdateEmail(Email email)
    {
        Email = email;
    }
    
    public void UpdateName(PersonName name)
    {
        Name = name;
    }
    
    public void UpdateRole(UserRole role)
    {
        Role = role;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
