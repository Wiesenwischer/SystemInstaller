using Microsoft.Extensions.DependencyInjection;
using SystemInstaller.IntegrationTests.TestBase;
using SystemInstaller.IntegrationTests.Utilities;
using SystemInstaller.Application.Interfaces;
using SystemInstaller.Application.DTOs;
using SystemInstaller.Domain.Enums;

namespace SystemInstaller.IntegrationTests.ApplicationTests;

public class UserInvitationApplicationServiceTests : BlazorComponentTestBase
{
    private readonly IUserInvitationApplicationService _invitationService;

    public UserInvitationApplicationServiceTests()
    {
        _invitationService = WebApplicationFactory.Services.GetRequiredService<IUserInvitationApplicationService>();
    }

    [Fact]
    public async Task CreateInvitationAsync_ShouldCreateNewInvitation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateUserInvitationDto
        {
            TenantId = tenant.Id,
            Email = "invite@test.com",
            Role = "Member"
        };

        // Act
        var result = await _invitationService.CreateInvitationAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("invite@test.com", result.Email);
        Assert.Equal("Member", result.Role);
        Assert.Equal("Pending", result.Status);
        Assert.NotNull(result.InvitationToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(result.Id);
        Assert.NotNull(dbInvitation);
        Assert.Equal(tenant.Id, dbInvitation.TenantId);
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ShouldReturnCorrectInvitation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Token Test Tenant");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "token@test.com");
        invitation.Status = InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.GetInvitationByTokenAsync(invitation.InvitationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invitation.Id, result.Id);
        Assert.Equal("token@test.com", result.Email);
        Assert.Equal("Token Test Tenant", result.TenantName);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        // Act
        var result = await _invitationService.GetInvitationByTokenAsync("invalid-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldAcceptInvitationAndCreateUser()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "accept@test.com");
        invitation.Status = InvitationStatus.Pending;
        invitation.Role = "Admin";

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto
        {
            InvitationToken = invitation.InvitationToken,
            UserEmail = "accept@test.com",
            UserName = "Accept User"
        };

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.True(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.Equal(InvitationStatus.Accepted, dbInvitation.Status);
        Assert.NotNull(dbInvitation.AcceptedAt);

        var user = DbContext.Users.FirstOrDefault(u => u.Email == "accept@test.com");
        Assert.NotNull(user);
        Assert.Equal("Accept User", user.Name);

        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && tu.UserId == user.Id);
        Assert.NotNull(tenantUser);
        Assert.Equal("Admin", tenantUser.Role);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldReturnFalse_WhenInvitationExpired()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "expired@test.com");
        invitation.Status = InvitationStatus.Pending;
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(-1); // Expired

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto
        {
            InvitationToken = invitation.InvitationToken,
            UserEmail = "expired@test.com",
            UserName = "Expired User"
        };

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.False(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.Equal(InvitationStatus.Pending, dbInvitation.Status); // Should remain unchanged
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldReturnFalse_WhenEmailMismatch()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "original@test.com");
        invitation.Status = InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto
        {
            InvitationToken = invitation.InvitationToken,
            UserEmail = "different@test.com",
            UserName = "Different User"
        };

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeclineInvitationAsync_ShouldDeclineInvitation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "decline@test.com");
        invitation.Status = InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.DeclineInvitationAsync(invitation.InvitationToken);

        // Assert
        Assert.True(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.Equal(InvitationStatus.Declined, dbInvitation.Status);

        // Ensure no user or tenant user relationship was created
        var user = DbContext.Users.FirstOrDefault(u => u.Email == "decline@test.com");
        Assert.Null(user);
    }

    [Fact]
    public async Task DeclineInvitationAsync_ShouldReturnFalse_WhenInvalidToken()
    {
        // Act
        var result = await _invitationService.DeclineInvitationAsync("invalid-token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPendingInvitationsForTenantAsync_ShouldReturnOnlyPendingInvitations()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        
        var pendingInvitation1 = TestDataFactory.CreateUserInvitation(tenant.Id, "pending1@test.com");
        pendingInvitation1.Status = InvitationStatus.Pending;
        
        var pendingInvitation2 = TestDataFactory.CreateUserInvitation(tenant.Id, "pending2@test.com");
        pendingInvitation2.Status = InvitationStatus.Pending;
        
        var acceptedInvitation = TestDataFactory.CreateUserInvitation(tenant.Id, "accepted@test.com");
        acceptedInvitation.Status = InvitationStatus.Accepted;
        
        var declinedInvitation = TestDataFactory.CreateUserInvitation(tenant.Id, "declined@test.com");
        declinedInvitation.Status = InvitationStatus.Declined;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddRangeAsync(
            pendingInvitation1, pendingInvitation2, acceptedInvitation, declinedInvitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.GetPendingInvitationsForTenantAsync(tenant.Id);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Email == "pending1@test.com");
        Assert.Contains(result, i => i.Email == "pending2@test.com");
        Assert.DoesNotContain(result, i => i.Email == "accepted@test.com");
        Assert.DoesNotContain(result, i => i.Email == "declined@test.com");
    }

    [Fact]
    public async Task CreateInvitationAsync_ShouldThrowException_WhenUserAlreadyInTenant()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var user = TestDataFactory.CreateUser("existing@test.com", "Existing User");
        var tenantUser = TestDataFactory.CreateTenantUser(tenant.Id, user.Id, "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(user);
        await DbContext.TenantUsers.AddAsync(tenantUser);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateUserInvitationDto
        {
            TenantId = tenant.Id,
            Email = "existing@test.com",
            Role = "Admin"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _invitationService.CreateInvitationAsync(createDto));
    }

    [Fact]
    public async Task CreateInvitationAsync_ShouldThrowException_WhenPendingInvitationExists()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var existingInvitation = TestDataFactory.CreateUserInvitation(tenant.Id, "pending@test.com");
        existingInvitation.Status = InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(existingInvitation);
        await DbContext.SaveChangesAsync();

        var createDto = new CreateUserInvitationDto
        {
            TenantId = tenant.Id,
            Email = "pending@test.com",
            Role = "Member"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _invitationService.CreateInvitationAsync(createDto));
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldUseExistingUser_WhenUserExists()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var existingUser = TestDataFactory.CreateUser("existing@test.com", "Existing User");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "existing@test.com");
        invitation.Status = InvitationStatus.Pending;
        invitation.Role = "Member";

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.Users.AddAsync(existingUser);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto
        {
            InvitationToken = invitation.InvitationToken,
            UserEmail = "existing@test.com",
            UserName = "New Name" // Different name
        };

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.True(result);

        var user = DbContext.Users.First(u => u.Email == "existing@test.com");
        Assert.Equal("Existing User", user.Name); // Should keep original name
        Assert.Equal(existingUser.Id, user.Id); // Should be same user

        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && tu.UserId == existingUser.Id);
        Assert.NotNull(tenantUser);
        Assert.Equal("Member", tenantUser.Role);
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ShouldIncludeTenantInformation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Detail Tenant", "Detail Description");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "detail@test.com");
        invitation.Role = "Admin";
        invitation.Status = InvitationStatus.Pending;

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.GetInvitationByTokenAsync(invitation.InvitationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Detail Tenant", result.TenantName);
        Assert.Equal("Admin", result.Role);
        Assert.Equal("detail@test.com", result.Email);
    }
}
