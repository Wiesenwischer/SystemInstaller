using System.ComponentModel.DataAnnotations;

namespace SystemInstaller.Web.Data
{
    public class Tenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public List<TenantUser> TenantUsers { get; set; } = new();
        public List<InstallationEnvironment> Environments { get; set; } = new();
    }
}
