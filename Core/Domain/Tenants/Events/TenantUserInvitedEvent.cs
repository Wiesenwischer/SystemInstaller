using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;

namespace SystemInstaller.Domain.Tenants.Events;

/// <summary>
/// Domain event raised when a user is invited to a tenant
/// </summary>
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
