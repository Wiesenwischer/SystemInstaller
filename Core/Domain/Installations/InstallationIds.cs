using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Installations;

/// <summary>
/// Strongly-typed identifier for Installation aggregate
/// </summary>
public sealed class InstallationId : Identity<Guid>
{
    public InstallationId(Guid value) : base(value)
    {
    }

    public static InstallationId New() => new(Guid.NewGuid());
    
    public static InstallationId From(Guid value) => new(value);
    
    public static implicit operator InstallationId(Guid value) => new(value);
    
    public static implicit operator Guid(InstallationId installationId) => installationId.Value;
}

/// <summary>
/// Strongly-typed identifier for InstallationTask entity
/// </summary>
public sealed class InstallationTaskId : Identity<Guid>
{
    public InstallationTaskId(Guid value) : base(value)
    {
    }

    public static InstallationTaskId New() => new(Guid.NewGuid());
    
    public static InstallationTaskId From(Guid value) => new(value);
    
    public static implicit operator InstallationTaskId(Guid value) => new(value);
    
    public static implicit operator Guid(InstallationTaskId installationTaskId) => installationTaskId.Value;
}

/// <summary>
/// Strongly-typed identifier for InstallationEnvironment entity
/// </summary>
public sealed class InstallationEnvironmentId : Identity<Guid>
{
    public InstallationEnvironmentId(Guid value) : base(value)
    {
    }

    public static InstallationEnvironmentId New() => new(Guid.NewGuid());
    
    public static InstallationEnvironmentId From(Guid value) => new(value);
    
    public static implicit operator InstallationEnvironmentId(Guid value) => new(value);
    
    public static implicit operator Guid(InstallationEnvironmentId environmentId) => environmentId.Value;
}
