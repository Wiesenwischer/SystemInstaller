using SystemInstaller.Domain.Enums;

namespace SystemInstaller.Domain.Entities;

public class InstallationTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstallationId { get; set; }
    public Guid EnvironmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InstallationStatus Status { get; set; } = InstallationStatus.Pending;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int Progress { get; set; } = 0; // 0-100
    public List<string> Logs { get; set; } = new();
    
    // Navigation properties
    public Installation Installation { get; set; } = null!;
    public InstallationEnvironment Environment { get; set; } = null!;

    // Constructor for creating a new task
    public InstallationTask(Guid environmentId, string name, string description)
    {
        Id = Guid.NewGuid();
        EnvironmentId = environmentId;
        Name = name;
        Description = description;
        Status = InstallationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Progress = 0;
        Logs = new List<string>();
    }

    // Parameterless constructor for EF Core
    public InstallationTask()
    {
        Logs = new List<string>();
    }

    // Domain methods
    public void Start()
    {
        if (Status != InstallationStatus.Pending)
            throw new InvalidOperationException($"Cannot start task in {Status} state");
            
        Status = InstallationStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != InstallationStatus.Running)
            throw new InvalidOperationException($"Cannot complete task in {Status} state");
            
        Status = InstallationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Progress = 100;
    }

    public void Fail(string errorMessage)
    {
        Status = InstallationStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InstallationStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed task");
            
        Status = InstallationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(int progress)
    {
        if (progress < 0 || progress > 100)
            throw new ArgumentException("Progress must be between 0 and 100");
            
        Progress = progress;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty");
            
        Name = name;
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
    }
}
