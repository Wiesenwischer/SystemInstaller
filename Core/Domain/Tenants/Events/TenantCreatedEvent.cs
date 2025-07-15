using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants.Model;

namespace SystemInstaller.Domain.Tenants.Events;

/// <summary>
/// Domain event raised when a tenant is created
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
