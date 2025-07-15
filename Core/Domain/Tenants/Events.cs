using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants;

/// <summary>
/// Domain events for tenant management
/// </summary>
public class TenantCreatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string TenantName { get; }
    public Email ContactEmail { get; }

    public TenantCreatedEvent(Guid tenantId, string tenantName, Email contactEmail)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        ContactEmail = contactEmail;
    }
}

public class TenantUserInvitedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid InvitationId { get; }
    public Email InviteeEmail { get; }
    public PersonName InviteeName { get; }

    public TenantUserInvitedEvent(Guid tenantId, Guid invitationId, Email inviteeEmail, PersonName inviteeName)
    {
        TenantId = tenantId;
        InvitationId = invitationId;
        InviteeEmail = inviteeEmail;
        InviteeName = inviteeName;
    }
}

public class UserInvitationAcceptedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid InvitationId { get; }
    public string UserId { get; }

    public UserInvitationAcceptedEvent(Guid tenantId, Guid invitationId, string userId)
    {
        TenantId = tenantId;
        InvitationId = invitationId;
        UserId = userId;
    }
}
