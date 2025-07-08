using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Domain.Entities;

public class UserInvitation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Email Email { get; private set; }
    public PersonName Name { get; private set; }
    public UserRole Role { get; private set; }
    public string InvitationToken { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public string? InvitedByUserId { get; private set; }
    
    // Navigation Properties
    public Tenant Tenant { get; private set; } = null!;
    
    private UserInvitation() { } // EF Core
    
    public UserInvitation(Guid tenantId, Email email, PersonName name, UserRole role, string? invitedByUserId = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Email = email;
        Name = name;
        Role = role;
        InvitationToken = Guid.NewGuid().ToString();
        IsUsed = false;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(7);
        InvitedByUserId = invitedByUserId;
    }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
    
    public void Use()
    {
        if (IsUsed)
            throw new InvalidOperationException("Invitation has already been used");
        
        if (IsExpired)
            throw new InvalidOperationException("Invitation has expired");
        
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
    
    public void ExtendExpiration(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));
        
        if (IsUsed)
            throw new InvalidOperationException("Cannot extend used invitation");
        
        ExpiresAt = DateTime.UtcNow.AddDays(days);
    }
    
    public void UpdateRole(UserRole role)
    {
        if (IsUsed)
            throw new InvalidOperationException("Cannot update role of used invitation");
        
        Role = role;
    }
}
