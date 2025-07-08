namespace SystemInstaller.Domain.Entities;

public class InstallationEnvironment
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    private readonly List<InstallationTask> _tasks = new();
    
    // Navigation Properties
    public Tenant Tenant { get; private set; } = null!;
    public IReadOnlyList<InstallationTask> Tasks => _tasks.AsReadOnly();
    
    private InstallationEnvironment() { } // EF Core
    
    public InstallationEnvironment(Guid tenantId, string name, string? description = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Environment name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Environment name cannot exceed 100 characters", nameof(name));
        
        if (description?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Environment name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Environment name cannot exceed 100 characters", nameof(name));
        
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateDescription(string? description)
    {
        if (description?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
        
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddTask(InstallationTask task)
    {
        _tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            _tasks.Remove(task);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void UpdateTask(Guid taskId, string name, string description)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            task.UpdateName(name);
            task.UpdateDescription(description);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
