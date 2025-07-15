using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants;

/// <summary>
/// Domain events for tenant management
/// </summary>
public class TenantCreatedEvent : DomainEvent
{
    public TenantId TenantId { get; }
    public string TenantName { get; }
    public Email ContactEmail { get; }

    public TenantCreatedEvent(TenantId tenantId, string tenantName, Email contactEmail)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        ContactEmail = contactEmail;
    }
}

public class TenantUserInvitedEvent : DomainEvent
{
    public TenantId TenantId { get; }
    public UserInvitationId InvitationId { get; }
    public Email InviteeEmail { get; }
    public PersonName InviteeName { get; }
    public UserRole InviteeRole { get; }

    public TenantUserInvitedEvent(TenantId tenantId, UserInvitationId invitationId, Email inviteeEmail, PersonName inviteeName, UserRole inviteeRole)
    {
        TenantId = tenantId;
        InvitationId = invitationId;
        InviteeEmail = inviteeEmail;
        InviteeName = inviteeName;
        InviteeRole = inviteeRole;
    }
}

public class UserInvitationAcceptedEvent : DomainEvent
{
    public TenantId TenantId { get; }
    public UserInvitationId InvitationId { get; }
    public string UserId { get; }

    public UserInvitationAcceptedEvent(TenantId tenantId, UserInvitationId invitationId, string userId)
    {
        TenantId = tenantId;
        InvitationId = invitationId;
        UserId = userId;
    }
}
