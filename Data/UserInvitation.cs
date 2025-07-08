using System.ComponentModel.DataAnnotations;

namespace SystemInstaller.Web.Data
{
    public class UserInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid TenantId { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Customer";
        
        [Required]
        public string InvitationToken { get; set; } = Guid.NewGuid().ToString();
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
        
        public DateTime? UsedAt { get; set; }
        
        [StringLength(100)]
        public string? InvitedByUserId { get; set; }
        
        // Navigation Properties
        public Tenant Tenant { get; set; } = null!;
    }
}
