using SystemInstaller.Domain.Entities;
using SystemInstaller.Application.DTOs;

namespace SystemInstaller.IntegrationTests.Utilities;

public static class TestDataFactory
{
    public static Tenant CreateTenant(string name = "Test Tenant", string description = "Test Description")
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static InstallationEnvironment CreateEnvironment(Guid tenantId, string name = "Test Environment")
    {
        return new InstallationEnvironment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = "Test Environment Description",
            ServerUrl = "https://test.example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Installation CreateInstallation(Guid environmentId, string version = "1.0.0")
    {
        return new Installation
        {
            Id = Guid.NewGuid(),
            EnvironmentId = environmentId,
            Version = version,
            Status = Domain.Enums.InstallationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Tasks = new List<InstallationTask>()
        };
    }

    public static InstallationTask CreateInstallationTask(Guid installationId, string name = "Test Task")
    {
        return new InstallationTask
        {
            Id = Guid.NewGuid(),
            InstallationId = installationId,
            Name = name,
            Description = "Test Task Description",
            Status = Domain.Enums.InstallationStatus.Pending,
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(string email = "test@example.com", string name = "Test User")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserInvitation CreateUserInvitation(Guid tenantId, string email = "invite@example.com")
    {
        return new UserInvitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            Role = "Member",
            Status = Domain.Enums.InvitationStatus.Pending,
            InvitationToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static TenantUser CreateTenantUser(Guid tenantId, Guid userId, string role = "Member")
    {
        return new TenantUser
        {
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };
    }

    public static List<Tenant> CreateMultipleTenants(int count = 3)
    {
        var tenants = new List<Tenant>();
        for (int i = 1; i <= count; i++)
        {
            tenants.Add(CreateTenant($"Tenant {i}", $"Description for Tenant {i}"));
        }
        return tenants;
    }

    public static List<InstallationEnvironment> CreateMultipleEnvironments(Guid tenantId, int count = 2)
    {
        var environments = new List<InstallationEnvironment>();
        for (int i = 1; i <= count; i++)
        {
            environments.Add(CreateEnvironment(tenantId, $"Environment {i}"));
        }
        return environments;
    }
}
