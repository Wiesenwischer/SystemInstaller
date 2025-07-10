using SystemInstaller.Domain.Entities;
using SystemInstaller.Domain.ValueObjects;
using SystemInstaller.Domain.Enums;
using SystemInstaller.Application.DTOs;

namespace SystemInstaller.IntegrationTests.Utilities;

public static class TestDataFactory
{
    public static Tenant CreateTenant(string name = "Test Tenant", string description = "Test Description")
    {
        var email = new Email("test@example.com");
        return new Tenant(name, email, description);
    }

    public static InstallationEnvironment CreateEnvironment(Guid tenantId, string name = "Test Environment")
    {
        return new InstallationEnvironment(tenantId, name, "Test Environment Description");
    }

    public static InstallationTask CreateInstallationTask(Guid environmentId, string name = "Test Task")
    {
        return new InstallationTask(environmentId, name, "Test Task Description");
    }

    public static Installation CreateInstallation(Guid environmentId, string version = "1.0.0")
    {
        return new Installation
        {
            Id = Guid.NewGuid(),
            EnvironmentId = environmentId,
            Version = version,
            Status = InstallationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(string email = "test@example.com", string firstName = "Test", string lastName = "User")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = $"{firstName} {lastName}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserInvitation CreateUserInvitation(Guid tenantId, string email = "invite@example.com", string firstName = "John", string lastName = "Doe")
    {
        var emailVO = new Email(email);
        var name = new PersonName(firstName, lastName);
        var role = UserRole.Member;
        return new UserInvitation(tenantId, emailVO, name, role);
    }

    public static TenantUser CreateTenantUser(Guid tenantId, string userId, string email = "user@example.com", string firstName = "Test", string lastName = "User", string role = "Member")
    {
        var emailVO = new Email(email);
        var name = new PersonName(firstName, lastName);
        var userRole = new UserRole(role);
        return new TenantUser(tenantId, userId, emailVO, name, userRole);
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
