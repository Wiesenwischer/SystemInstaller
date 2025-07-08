using System.ComponentModel.DataAnnotations;

namespace SystemInstaller.Web.Data
{
    public class TenantUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid TenantId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty; // Keycloak User ID
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Customer"; // Admin, Customer
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation Properties
        public Tenant Tenant { get; set; } = null!;
    }
}
