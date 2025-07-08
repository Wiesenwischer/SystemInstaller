using System.ComponentModel.DataAnnotations;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.Web.Data;

public class InstallationTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid EnvironmentId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public InstallationStatus Status { get; set; } = InstallationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public List<string> Logs { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
    
    public int Progress { get; set; } = 0; // 0-100
    
    // Navigation Properties
    public InstallationEnvironment Environment { get; set; } = null!;
}