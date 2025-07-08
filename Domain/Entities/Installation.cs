using SystemInstaller.Domain.Enums;

namespace SystemInstaller.Domain.Entities;

public class Installation
{
    public Guid Id { get; set; }
    public Guid EnvironmentId { get; set; }
    public string Version { get; set; } = string.Empty;
    public InstallationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public InstallationEnvironment Environment { get; set; } = null!;
    public ICollection<InstallationTask> Tasks { get; set; } = new List<InstallationTask>();
}
