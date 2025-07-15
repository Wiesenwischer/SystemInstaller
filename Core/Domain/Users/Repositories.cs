using SystemInstaller.SharedKernel;
using SystemInstaller.Domain.Tenants; // For Email

namespace SystemInstaller.Domain.Users;

/// <summary>
/// Repository interface for UserRegistration aggregate
/// </summary>
public interface IUserRegistrationRepository : IRepository<UserRegistration, UserRegistrationId>
{
    Task<UserRegistration?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<UserRegistration?> GetByVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<List<UserRegistration>> GetExpiredRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<List<UserRegistration>> GetByStatusAsync(RegistrationStatus status, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for User aggregate
/// </summary>
public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetInactiveUsersAsync(CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(CancellationToken cancellationToken = default);
}
