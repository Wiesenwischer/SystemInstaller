namespace SystemInstaller.Core.Domain.Enums;

/// <summary>
/// Legacy enum - use SystemInstaller.Core.Domain.InstallationManagement.InstallationStatus instead
/// </summary>
[Obsolete("Use SystemInstaller.Core.Domain.InstallationManagement.InstallationStatus instead")]
public enum InstallationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
