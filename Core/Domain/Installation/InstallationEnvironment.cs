using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.Installation;

/// <summary>
/// Represents an installation environment where installations can be executed
/// </summary>
public class InstallationEnvironment : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? ServerUrl { get; private set; }
    public string? DatabaseConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Collections
    private readonly List<InstallationTask> _tasks = new();
    public IReadOnlyList<InstallationTask> Tasks => _tasks.AsReadOnly();

    private InstallationEnvironment() { } // For EF Core

    public InstallationEnvironment(Guid tenantId, string name, string? description, string? serverUrl, string? databaseConnectionString)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        ServerUrl = serverUrl;
        DatabaseConnectionString = databaseConnectionString;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string? serverUrl, string? databaseConnectionString)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        ServerUrl = serverUrl;
        DatabaseConnectionString = databaseConnectionString;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void AddTask(InstallationTask task)
    {
        _tasks.Add(task);
    }
}
