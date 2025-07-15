namespace SystemInstaller.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a tenant is created
/// Uses primitive types for external consumption
/// </summary>
public record TenantCreatedIntegrationEvent(
    Guid TenantId,
    string Name,
    string ContactEmail,
    DateTime CreatedAt);

/// <summary>
/// Integration event published when a user is invited to a tenant
/// Uses primitive types for external consumption
/// </summary>
public record TenantUserInvitedIntegrationEvent(
    Guid TenantId,
    Guid InvitationId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime InvitedAt);

/// <summary>
/// Integration event published when a user invitation is accepted
/// Uses primitive types for external consumption
/// </summary>
public record UserInvitationAcceptedIntegrationEvent(
    Guid TenantId,
    Guid InvitationId,
    string UserId,
    string Email,
    DateTime AcceptedAt);
