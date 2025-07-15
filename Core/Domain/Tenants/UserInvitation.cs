using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants;

/// <summary>
/// Represents a user invitation to join a tenant
/// </summary>
public class UserInvitation : Entity<UserInvitationId>
{
    public TenantId TenantId { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PersonName Name { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public string InvitationToken { get; private set; } = default!;
    public string? InvitedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;

    private UserInvitation() { } // For EF Core

    internal UserInvitation(TenantId tenantId, Email email, PersonName name, UserRole role, string invitedByUserId)
    {
        Id = UserInvitationId.New();
        TenantId = tenantId;
        Email = email;
        Name = name;
        Role = role;
        InvitationToken = GenerateToken();
        InvitedByUserId = invitedByUserId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(7); // 7 days expiration
        IsUsed = false;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkAsUsed()
    {
        if (!IsValid)
            throw new BusinessRuleViolationException("Cannot use an invalid invitation");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N")[..16]; // 16 character token
    }
}
