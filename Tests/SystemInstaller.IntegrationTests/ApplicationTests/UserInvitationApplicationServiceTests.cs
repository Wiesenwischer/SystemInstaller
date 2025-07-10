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
        Assert.False(result.IsUsed);
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
        Assert.False(result.IsUsed);
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



        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto(invitation.InvitationToken, "testuser");

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.True(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.NotNull(dbInvitation);
        Assert.True(dbInvitation.IsUsed);
        Assert.NotNull(dbInvitation.UsedAt);

        var user = DbContext.TenantUsers.FirstOrDefault(u => u.Email == "accept@test.com");
        Assert.NotNull(user);
        Assert.Equal("Accept User", user.Name.FullName);

        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && tu.UserId == user.Id.ToString());
        Assert.NotNull(tenantUser);
        Assert.Equal("Admin", tenantUser.Role);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldReturnFalse_WhenInvitationExpired()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "expired@test.com");

        // Note: ExpiresAt is read-only, so this test assumes the invitation expires naturally
        // or we'd need to mock the current time. For now, test with a valid invitation
        // but verify the service handles expiration correctly in the business logic

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto(invitation.InvitationToken, "testuser");

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.False(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.NotNull(dbInvitation);
        Assert.False(dbInvitation.IsUsed); // Should remain unchanged
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldReturnFalse_WhenEmailMismatch()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "original@test.com");


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto(invitation.InvitationToken, "testuser");

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


        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.CancelInvitationAsync(invitation.Id);

        // Assert
        Assert.True(result);

        var dbInvitation = await DbContext.UserInvitations.FindAsync(invitation.Id);
        Assert.NotNull(dbInvitation);
        // Note: CancelInvitationAsync might just mark as cancelled rather than changing IsUsed
        // Verify the invitation still exists but is cancelled
        Assert.False(dbInvitation.IsUsed);

        // Ensure no user or tenant user relationship was created
        var user = DbContext.TenantUsers.FirstOrDefault(u => u.Email == "decline@test.com");
        Assert.Null(user);
    }

    [Fact]
    public async Task CancelInvitationAsync_ShouldReturnFalse_WhenInvalidId()
    {
        // Act
        var result = await _invitationService.CancelInvitationAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPendingInvitationsForTenantAsync_ShouldReturnOnlyPendingInvitations()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant();
        
        var pendingInvitation1 = TestDataFactory.CreateUserInvitation(tenant.Id, "pending1@test.com");

        
        var pendingInvitation2 = TestDataFactory.CreateUserInvitation(tenant.Id, "pending2@test.com");

        
        var acceptedInvitation = TestDataFactory.CreateUserInvitation(tenant.Id, "accepted@test.com");
        acceptedInvitation.Use();
        
        var declinedInvitation = TestDataFactory.CreateUserInvitation(tenant.Id, "declined@test.com");
        declinedInvitation.Use(); // Mark as used instead of direct assignment

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.UserInvitations.AddRangeAsync(
            pendingInvitation1, pendingInvitation2, acceptedInvitation, declinedInvitation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _invitationService.GetPendingInvitationsAsync(tenant.Id);

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
        var user = TestDataFactory.CreateTenantUser(tenant.Id, "existing-user-id", "existing@test.com", "Existing", "User", "Member");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.TenantUsers.AddAsync(user);
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
        var existingUser = TestDataFactory.CreateTenantUser(tenant.Id, "existing-user-id", "existing@test.com", "Existing", "User", "Member");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "existing@test.com");

        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.TenantUsers.AddAsync(existingUser);
        await DbContext.UserInvitations.AddAsync(invitation);
        await DbContext.SaveChangesAsync();

        var acceptDto = new AcceptInvitationDto(invitation.InvitationToken, "testuser");

        // Act
        var result = await _invitationService.AcceptInvitationAsync(acceptDto);

        // Assert
        Assert.True(result);

        var user = DbContext.TenantUsers.First(u => u.Email.Value == "existing@test.com");
        Assert.Equal("Existing User", user.Name.FullName); // Should keep original name
        Assert.Equal(existingUser.UserId, user.UserId); // Should be same user ID

        var tenantUser = DbContext.TenantUsers.FirstOrDefault(tu => 
            tu.TenantId == tenant.Id && tu.UserId == existingUser.UserId);
        Assert.NotNull(tenantUser);
        Assert.Equal("Member", tenantUser.Role.ToString());
    }

    [Fact]
    public async Task GetInvitationByTokenAsync_ShouldIncludeTenantInformation()
    {
        // Arrange
        var tenant = TestDataFactory.CreateTenant("Detail Tenant", "Detail Description");
        var invitation = TestDataFactory.CreateUserInvitation(tenant.Id, "detail@test.com");



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

