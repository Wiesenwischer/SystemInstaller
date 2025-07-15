using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.TenantManagement;

/// <summary>
/// Tenant aggregate root - manages tenant lifecycle and user management
/// </summary>
public class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Email ContactEmail { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Collections
    private readonly List<TenantUser> _tenantUsers = new();
    private readonly List<UserInvitation> _userInvitations = new();

    public IReadOnlyList<TenantUser> TenantUsers => _tenantUsers.AsReadOnly();
    public IReadOnlyList<UserInvitation> UserInvitations => _userInvitations.AsReadOnly();

    private Tenant() { } // For EF Core

    public Tenant(string name, string? description, Email contactEmail)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        ContactEmail = contactEmail ?? throw new ArgumentNullException(nameof(contactEmail));
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TenantCreatedEvent(Id, Name, ContactEmail));
    }

    public void UpdateDetails(string name, string? description, Email contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
        Description = description;
        ContactEmail = contactEmail ?? throw new ArgumentNullException(nameof(contactEmail));
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
    }

    public UserInvitation InviteUser(Email email, PersonName name, UserRole role, string invitedByUserId)
    {
        // Business rule: Cannot invite if user already exists
        if (_tenantUsers.Any(u => u.Email == email))
            throw new BusinessRuleViolationException($"User with email {email} already exists in tenant");

        // Business rule: Cannot have multiple pending invitations for same email
        if (_userInvitations.Any(i => i.Email == email && i.IsValid))
            throw new BusinessRuleViolationException($"Pending invitation already exists for {email}");

        var invitation = new UserInvitation(Id, email, name, role, invitedByUserId);
        _userInvitations.Add(invitation);

        AddDomainEvent(new TenantUserInvitedEvent(Id, invitation.Id, email, name));
        IncrementVersion();

        return invitation;
    }

    public TenantUser AcceptInvitation(Guid invitationId, string userId)
    {
        var invitation = _userInvitations.FirstOrDefault(i => i.Id == invitationId);
        if (invitation == null)
            throw new EntityNotFoundException(nameof(UserInvitation), invitationId);

        if (!invitation.IsValid)
            throw new BusinessRuleViolationException("Invitation is no longer valid");

        invitation.MarkAsUsed();

        var tenantUser = new TenantUser(Id, userId, invitation.Email, invitation.Name, invitation.Role);
        _tenantUsers.Add(tenantUser);

        AddDomainEvent(new UserInvitationAcceptedEvent(Id, invitationId, userId));
        IncrementVersion();

        return tenantUser;
    }

    public void RemoveUser(Guid userId)
    {
        var user = _tenantUsers.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(TenantUser), userId);

        _tenantUsers.Remove(user);
        IncrementVersion();
    }

    public void DeactivateUser(Guid userId)
    {
        var user = _tenantUsers.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(TenantUser), userId);

        user.Deactivate();
        IncrementVersion();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        IncrementVersion();
    }
}
