namespace SystemInstaller.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string? Description,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int UserCount,
    int EnvironmentCount
);

public record CreateTenantDto(
    string Name,
    string ContactEmail,
    string? Description
);

public record UpdateTenantDto(
    string Name,
    string? Description,
    string ContactEmail
);

public record TenantEnvironmentDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);

public record TenantDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<TenantUserDto> TenantUsers,
    List<TenantEnvironmentDto> Environments
);
