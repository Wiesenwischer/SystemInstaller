using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Users.Model;

/// <summary>
/// Strongly-typed identifier for UserRegistration aggregate
/// </summary>
public sealed class UserRegistrationId : Identity<Guid>
{
    public UserRegistrationId(Guid value) : base(value)
    {
    }

    public static UserRegistrationId New() => new(Guid.NewGuid());
    
    public static UserRegistrationId From(Guid value) => new(value);
    
    public static implicit operator UserRegistrationId(Guid value) => new(value);
    
    public static implicit operator Guid(UserRegistrationId registrationId) => registrationId.Value;
}

/// <summary>
/// Strongly-typed identifier for verified User
/// </summary>
public sealed class UserId : Identity<Guid>
{
    public UserId(Guid value) : base(value)
    {
    }

    public static UserId New() => new(Guid.NewGuid());
    
    public static UserId From(Guid value) => new(value);
    
    public static implicit operator UserId(Guid value) => new(value);
    
    public static implicit operator Guid(UserId userId) => userId.Value;
}
