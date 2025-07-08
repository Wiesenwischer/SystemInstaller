using Microsoft.EntityFrameworkCore;
using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Infrastructure.Data;

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
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            
            // Value Object mapping
            entity.OwnsOne(e => e.ContactEmail, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("ContactEmail")
                    .HasMaxLength(100)
                    .IsRequired();
            });
            
            // Configure collections
            entity.HasMany(e => e.TenantUsers)
                .WithOne(tu => tu.Tenant)
                .HasForeignKey(tu => tu.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Environments)
                .WithOne(env => env.Tenant)
                .HasForeignKey(env => env.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TenantUser Configuration
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastLoginAt);
            
            // Value Object mappings
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(100)
                    .IsRequired();
            });
            
            entity.OwnsOne(e => e.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .HasMaxLength(50)
                    .IsRequired();
                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .HasMaxLength(50)
                    .IsRequired();
            });
            
            entity.OwnsOne(e => e.Role, role =>
            {
                role.Property(r => r.Value)
                    .HasColumnName("Role")
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

        // UserInvitation Configuration
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvitationToken).IsUnique();
            entity.Property(e => e.InvitationToken).IsRequired();
            entity.Property(e => e.IsUsed).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.UsedAt);
            entity.Property(e => e.InvitedByUserId).HasMaxLength(100);
            
            // Value Object mappings
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(100)
                    .IsRequired();
            });
            
            entity.OwnsOne(e => e.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .HasMaxLength(50)
                    .IsRequired();
                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .HasMaxLength(50)
                    .IsRequired();
            });
            
            entity.OwnsOne(e => e.Role, role =>
            {
                role.Property(r => r.Value)
                    .HasColumnName("Role")
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

        // InstallationEnvironment Configuration
        modelBuilder.Entity<InstallationEnvironment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
            
            // Configure Tasks navigation property
            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.Environment)
                .HasForeignKey(t => t.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // InstallationTask Configuration
        modelBuilder.Entity<InstallationTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.StartedAt);
            entity.Property(e => e.CompletedAt);
            entity.Property(e => e.ErrorMessage);
            entity.Property(e => e.Progress).IsRequired();
            
            // Configure Logs as JSON column
            entity.Property(e => e.Logs)
                .HasConversion(
                    logs => System.Text.Json.JsonSerializer.Serialize(logs, (System.Text.Json.JsonSerializerOptions?)null),
                    logs => System.Text.Json.JsonSerializer.Deserialize<List<string>>(logs, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");
        });
    }
}
