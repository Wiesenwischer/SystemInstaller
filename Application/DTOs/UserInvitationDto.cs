namespace SystemInstaller.Application.DTOs;

public record UserInvitationDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    string InvitationToken,
    bool IsUsed,
    bool IsExpired,
    bool IsValid,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? UsedAt,
    string? InvitedByUserId
);

public record CreateInvitationDto(
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? InvitedByUserId
);

public record AcceptInvitationDto(
    string Token,
    string UserId
);
