using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemInstaller.Domain.Tenants;

namespace SystemInstaller.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        
        // Configure strongly-typed ID
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .HasColumnName("Id");

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        // Configure Email value object
        builder.OwnsOne(t => t.ContactEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("ContactEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        // Configure collections
        builder.HasMany<TenantUser>()
            .WithOne()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<UserInvitation>()
            .WithOne()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on name
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}

public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("TenantUsers");
        
        // Configure strongly-typed ID
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new TenantUserId(value))
            .HasColumnName("Id");

        // Configure TenantId as foreign key
        builder.Property("TenantId")
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .HasColumnName("TenantId");

        builder.Property(u => u.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // Configure Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();
        });

        // Configure PersonName value object
        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
            
            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Unique constraint on TenantId + UserId
        builder.HasIndex("TenantId", "UserId")
            .IsUnique();
    }
}

public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("UserInvitations");
        
        // Configure strongly-typed ID
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new UserInvitationId(value))
            .HasColumnName("Id");

        // Configure TenantId as foreign key
        builder.Property("TenantId")
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .HasColumnName("TenantId");

        builder.Property(i => i.InvitationToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.InvitedByUserId)
            .HasMaxLength(50);

        builder.Property(i => i.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.IsUsed)
            .IsRequired();

        builder.Property(i => i.UsedAt);

        // Configure Email value object
        builder.OwnsOne(i => i.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();
        });

        // Configure PersonName value object
        builder.OwnsOne(i => i.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();
            
            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Index on invitation token
        builder.HasIndex(i => i.InvitationToken)
            .IsUnique();
    }
}
