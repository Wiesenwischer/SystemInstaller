using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemInstaller.Domain.Installations;

namespace SystemInstaller.Infrastructure.Persistence.Configurations;

public class InstallationConfiguration : IEntityTypeConfiguration<Installation>
{
    public void Configure(EntityTypeBuilder<Installation> builder)
    {
        builder.ToTable("Installations");
        
        // Configure strongly-typed ID
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new InstallationId(value))
            .HasColumnName("Id");

        // Configure EnvironmentId
        builder.Property(i => i.EnvironmentId)
            .HasConversion(
                id => id.Value,
                value => new InstallationEnvironmentId(value))
            .HasColumnName("EnvironmentId")
            .IsRequired();

        builder.Property(i => i.ProductVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.StartedAt);

        builder.Property(i => i.CompletedAt);

        builder.Property(i => i.ErrorMessage)
            .HasMaxLength(2000);

        // Configure collections
        builder.HasMany<InstallationTask>()
            .WithOne()
            .HasForeignKey("InstallationId")
            .OnDelete(DeleteBehavior.Cascade);

        // Index on environment and status
        builder.HasIndex(i => new { i.EnvironmentId, i.Status });
    }
}

public class InstallationTaskConfiguration : IEntityTypeConfiguration<InstallationTask>
{
    public void Configure(EntityTypeBuilder<InstallationTask> builder)
    {
        builder.ToTable("InstallationTasks");
        
        // Configure strongly-typed ID
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new InstallationTaskId(value))
            .HasColumnName("Id");

        // Configure InstallationId as foreign key
        builder.Property("InstallationId")
            .HasConversion(
                id => id.Value,
                value => new InstallationId(value))
            .HasColumnName("InstallationId");

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Command)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Arguments)
            .HasMaxLength(1000);

        builder.Property(t => t.WorkingDirectory)
            .HasMaxLength(500);

        builder.Property(t => t.OrderIndex)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.StartedAt);

        builder.Property(t => t.CompletedAt);

        builder.Property(t => t.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(t => t.OutputLog)
            .HasMaxLength(10000);

        // Index on InstallationId and OrderIndex
        builder.HasIndex("InstallationId", "OrderIndex")
            .IsUnique();
    }
}

public class InstallationEnvironmentConfiguration : IEntityTypeConfiguration<InstallationEnvironment>
{
    public void Configure(EntityTypeBuilder<InstallationEnvironment> builder)
    {
        builder.ToTable("InstallationEnvironments");
        
        // Configure strongly-typed ID
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => new InstallationEnvironmentId(value))
            .HasColumnName("Id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.ServerUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        // Unique constraint on name
        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
