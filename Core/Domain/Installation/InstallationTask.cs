using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.Installation;

/// <summary>
/// Represents an installation task with execution details
/// </summary>
public class InstallationTask : Entity<Guid>
{
    public Guid InstallationId { get; private set; }
    public Guid EnvironmentId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string ScriptPath { get; private set; } = default!;
    public string Parameters { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public TaskStatus Status { get; private set; }
    public int Progress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Logs { get; private set; } = new();

    // Navigation properties
    public Installation Installation { get; private set; } = default!;
    public InstallationEnvironment Environment { get; private set; } = default!;

    private InstallationTask() { } // For EF Core

    internal InstallationTask(Guid installationId, Guid environmentId, string name, string description, 
        string scriptPath, string parameters, int order)
    {
        Id = Guid.NewGuid();
        InstallationId = installationId;
        EnvironmentId = environmentId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        ScriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
        Parameters = parameters ?? string.Empty;
        Order = order;
        Status = TaskStatus.Pending;
        Progress = 0;
        CreatedAt = DateTime.UtcNow;
        Logs = new List<string>();
    }

    public void Start()
    {
        if (Status != TaskStatus.Pending)
            throw new BusinessRuleViolationException($"Task must be in pending status to start. Current status: {Status}");

        Status = TaskStatus.Running;
        StartedAt = DateTime.UtcNow;
        Progress = 0;
        ErrorMessage = null;
    }

    public void UpdateProgress(int progress, string? logMessage = null)
    {
        if (Status != TaskStatus.Running)
            throw new BusinessRuleViolationException($"Can only update progress for running tasks. Current status: {Status}");

        if (progress < 0 || progress > 100)
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 100");

        Progress = progress;
        
        if (!string.IsNullOrEmpty(logMessage))
        {
            Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logMessage}");
        }
    }

    public void Complete()
    {
        if (Status != TaskStatus.Running)
            throw new BusinessRuleViolationException($"Task must be running to complete. Current status: {Status}");

        Status = TaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Progress = 100;
        Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Task completed successfully");
    }

    public void Fail(string errorMessage)
    {
        if (Status != TaskStatus.Running)
            throw new BusinessRuleViolationException($"Task must be running to fail. Current status: {Status}");

        Status = TaskStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Task failed: {errorMessage}");
    }

    public void Skip(string reason)
    {
        if (Status != TaskStatus.Pending)
            throw new BusinessRuleViolationException($"Only pending tasks can be skipped. Current status: {Status}");

        Status = TaskStatus.Skipped;
        CompletedAt = DateTime.UtcNow;
        Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Task skipped: {reason}");
    }

    public bool IsFinished => Status == TaskStatus.Completed || Status == TaskStatus.Failed || Status == TaskStatus.Skipped;
}
