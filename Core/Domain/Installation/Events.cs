using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.Installation;

/// <summary>
/// Installation status enumeration
/// </summary>
public enum InstallationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Installation task status enumeration
/// </summary>
public enum TaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped
}

/// <summary>
/// Domain events for installation management
/// </summary>
public class InstallationStartedEvent : DomainEvent
{
    public Guid InstallationId { get; }
    public Guid EnvironmentId { get; }

    public InstallationStartedEvent(Guid installationId, Guid environmentId)
    {
        InstallationId = installationId;
        EnvironmentId = environmentId;
    }
}

public class InstallationCompletedEvent : DomainEvent
{
    public Guid InstallationId { get; }
    public bool Success { get; }

    public InstallationCompletedEvent(Guid installationId, bool success)
    {
        InstallationId = installationId;
        Success = success;
    }
}

public class InstallationTaskCompletedEvent : DomainEvent
{
    public Guid InstallationId { get; }
    public Guid TaskId { get; }
    public TaskStatus Status { get; }

    public InstallationTaskCompletedEvent(Guid installationId, Guid taskId, TaskStatus status)
    {
        InstallationId = installationId;
        TaskId = taskId;
        Status = status;
    }
}
