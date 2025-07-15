using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.Entities;

/// <summary>
/// Legacy InstallationEnvironment - should be migrated to new domain structure
/// </summary>
[Obsolete("Use SystemInstaller.Core.Domain.InstallationManagement.InstallationEnvironment instead")]
public class InstallationEnvironment : Entity<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public ICollection<InstallationTask> Tasks { get; set; } = new List<InstallationTask>();
}
