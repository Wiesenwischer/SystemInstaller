namespace SystemInstaller.Application.DTOs;

public record TenantUserDto(
    Guid Id,
    Guid TenantId,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record CreateTenantUserDto(
    Guid TenantId,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role
);

public record UpdateTenantUserDto(
    string Email,
    string FirstName,
    string LastName,
    string Role
);
