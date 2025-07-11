using SystemInstaller.Core.Domain.Enums;

namespace SystemInstaller.Core.Domain.Entities;

public class InstallationTask : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string ScriptPath { get; private set; } = default!;
    public string Parameters { get; private set; } = string.Empty;
    public InstallationStatus Status { get; private set; } = InstallationStatus.Pending;
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? LogOutput { get; private set; }
    public Guid EnvironmentId { get; private set; }
    public InstallationEnvironment Environment { get; private set; } = default!;

    private InstallationTask() { } // For EF Core

    public InstallationTask(string name, string description, string scriptPath, string parameters, Guid environmentId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        ScriptPath = scriptPath;
        Parameters = parameters;
        EnvironmentId = environmentId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != InstallationStatus.Pending)
            throw new InvalidOperationException("Task can only be started from Pending status");

        Status = InstallationStatus.Running;
        StartedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void Complete(string? logOutput = null)
    {
        if (Status != InstallationStatus.Running)
            throw new InvalidOperationException("Task can only be completed from Running status");

        Status = InstallationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        LogOutput = logOutput;
        ErrorMessage = null;
    }

    public void Fail(string errorMessage, string? logOutput = null)
    {
        if (Status != InstallationStatus.Running)
            throw new InvalidOperationException("Task can only be failed from Running status");

        Status = InstallationStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        LogOutput = logOutput;
    }

    public void Cancel()
    {
        if (Status == InstallationStatus.Completed || Status == InstallationStatus.Failed)
            throw new InvalidOperationException("Cannot cancel a completed or failed task");

        Status = InstallationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        Status = InstallationStatus.Pending;
        StartedAt = null;
        CompletedAt = null;
        ErrorMessage = null;
        LogOutput = null;
    }
}
