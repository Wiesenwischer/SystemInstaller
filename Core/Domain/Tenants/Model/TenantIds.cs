using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants.Model;

/// <summary>
/// Strongly-typed identifier for Tenant aggregate
/// </summary>
public sealed class TenantId : Identity<Guid>
{
    public TenantId(Guid value) : base(value)
    {
    }

    public static TenantId New() => new(Guid.NewGuid());
    
    public static TenantId From(Guid value) => new(value);
    
    public static implicit operator TenantId(Guid value) => new(value);
    
    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
}

/// <summary>
/// Strongly-typed identifier for TenantUser entity
/// </summary>
public sealed class TenantUserId : Identity<Guid>
{
    public TenantUserId(Guid value) : base(value)
    {
    }

    public static TenantUserId New() => new(Guid.NewGuid());
    
    public static TenantUserId From(Guid value) => new(value);
    
    public static implicit operator TenantUserId(Guid value) => new(value);
    
    public static implicit operator Guid(TenantUserId tenantUserId) => tenantUserId.Value;
}

/// <summary>
/// Strongly-typed identifier for UserInvitation entity
/// </summary>
public sealed class UserInvitationId : Identity<Guid>
{
    public UserInvitationId(Guid value) : base(value)
    {
    }

    public static UserInvitationId New() => new(Guid.NewGuid());
    
    public static UserInvitationId From(Guid value) => new(value);
    
    public static implicit operator UserInvitationId(Guid value) => new(value);
    
    public static implicit operator Guid(UserInvitationId userInvitationId) => userInvitationId.Value;
}
