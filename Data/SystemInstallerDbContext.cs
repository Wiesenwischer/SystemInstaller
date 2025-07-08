using Microsoft.EntityFrameworkCore;

namespace SystemInstaller.Web.Data
{
    public class SystemInstallerDbContext : DbContext
    {
        public SystemInstallerDbContext(DbContextOptions<SystemInstallerDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantUser> TenantUsers { get; set; }
        public DbSet<UserInvitation> UserInvitations { get; set; }
        public DbSet<InstallationEnvironment> Environments { get; set; }
        public DbSet<InstallationTask> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Tenant Configuration
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // TenantUser Configuration
            modelBuilder.Entity<TenantUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.TenantUsers)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserInvitation Configuration
            modelBuilder.Entity<UserInvitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.InvitationToken).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InvitationToken).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InstallationEnvironment Configuration
            modelBuilder.Entity<InstallationEnvironment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Environments)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InstallationTask Configuration
            modelBuilder.Entity<InstallationTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Environment)
                    .WithMany(env => env.Tasks)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
