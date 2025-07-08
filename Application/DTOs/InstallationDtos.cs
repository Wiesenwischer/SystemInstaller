using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Application.DTOs;

public class CreateUserInvitationDto
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? InvitedByUserId { get; set; }
}

public class UserInvitationResultDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string InvitationToken { get; set; } = null!;
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? InvitedByUserId { get; set; }
    public string? TenantName { get; set; }
}

public class AcceptInvitationDto
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
}

public class CreateInstallationTaskDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid EnvironmentId { get; set; }
}

public class InstallationTaskResultDto
{
    public Guid Id { get; set; }
    public Guid EnvironmentId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public InstallationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int Progress { get; set; }
    public List<string> Logs { get; set; } = new();
    public string? EnvironmentName { get; set; }
}

public class CreateInstallationEnvironmentDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class InstallationEnvironmentResultDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<InstallationTaskResultDto> Tasks { get; set; } = new();
}
