using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;

namespace SystemInstaller.Domain.Tenants.Events;

/// <summary>
/// Domain event raised when a user invitation is accepted
/// </summary>
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
