using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Domain.Entities;

public class InstallationTask
{
    public Guid Id { get; private set; }
    public Guid EnvironmentId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public InstallationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int Progress { get; private set; }
    
    private readonly List<string> _logs = new();
    
    // Navigation Properties
    public InstallationEnvironment Environment { get; private set; } = null!;
    public IReadOnlyList<string> Logs => _logs.AsReadOnly();
    
    private InstallationTask() { } // EF Core
    
    public InstallationTask(Guid environmentId, string name, string description)
    {
        if (environmentId == Guid.Empty)
            throw new ArgumentException("EnvironmentId cannot be empty", nameof(environmentId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Task name cannot exceed 100 characters", nameof(name));
        
        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
        
        Id = Guid.NewGuid();
        EnvironmentId = environmentId;
        Name = name;
        Description = description;
        Status = InstallationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Progress = 0;
    }
    
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Task name cannot exceed 100 characters", nameof(name));
        
        Name = name;
    }
    
    public void UpdateDescription(string description)
    {
        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
        
        Description = description;
    }
    
    public void Start()
    {
        if (Status != InstallationStatus.Pending)
            throw new InvalidOperationException($"Cannot start task in {Status} status");
        
        Status = InstallationStatus.Running;
        StartedAt = DateTime.UtcNow;
        Progress = 0;
    }
    
    public void UpdateProgress(int progress)
    {
        if (progress < 0 || progress > 100)
            throw new ArgumentException("Progress must be between 0 and 100", nameof(progress));
        
        Progress = progress;
    }
    
    public void Complete()
    {
        if (Status != InstallationStatus.Running)
            throw new InvalidOperationException($"Cannot complete task in {Status} status");
        
        Status = InstallationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Progress = 100;
    }
    
    public void Fail(string errorMessage)
    {
        if (Status != InstallationStatus.Running)
            throw new InvalidOperationException($"Cannot fail task in {Status} status");
        
        Status = InstallationStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
    
    public void Cancel()
    {
        if (Status != InstallationStatus.Running && Status != InstallationStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel task in {Status} status");
        
        Status = InstallationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;
        
        _logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
    }
    
    public void ClearLogs()
    {
        _logs.Clear();
    }
}
