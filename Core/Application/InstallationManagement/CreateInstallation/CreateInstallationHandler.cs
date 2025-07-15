using MediatR;
using SystemInstaller.Core.Domain.InstallationManagement;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Application.InstallationManagement.CreateInstallation;

public class CreateInstallationCommand : IRequest<CreateInstallationResult>
{
    public Guid EnvironmentId { get; set; }
    public string Version { get; set; } = default!;
    public List<InstallationTaskDto> Tasks { get; set; } = new();
}

public class InstallationTaskDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ScriptPath { get; set; } = default!;
    public string Parameters { get; set; } = string.Empty;
}

public class CreateInstallationResult
{
    public Guid InstallationId { get; set; }
    public Guid EnvironmentId { get; set; }
    public string Version { get; set; } = default!;
    public List<Guid> TaskIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateInstallationHandler : IRequestHandler<CreateInstallationCommand, CreateInstallationResult>
{
    private readonly IInstallationRepository _installationRepository;
    private readonly IInstallationEnvironmentRepository _environmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallationHandler(
        IInstallationRepository installationRepository,
        IInstallationEnvironmentRepository environmentRepository,
        IUnitOfWork unitOfWork)
    {
        _installationRepository = installationRepository;
        _environmentRepository = environmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInstallationResult> Handle(CreateInstallationCommand request, CancellationToken cancellationToken)
    {
        // Verify environment exists
        var environment = await _environmentRepository.GetByIdAsync(request.EnvironmentId, cancellationToken);
        if (environment == null)
        {
            throw new EntityNotFoundException(nameof(InstallationEnvironment), request.EnvironmentId);
        }

        // Create installation
        var installation = new Installation(request.EnvironmentId, request.Version);

        // Add tasks
        var taskIds = new List<Guid>();
        foreach (var taskDto in request.Tasks)
        {
            var task = installation.AddTask(taskDto.Name, taskDto.Description, taskDto.ScriptPath, taskDto.Parameters);
            taskIds.Add(task.Id);
        }

        // Save to repository
        await _installationRepository.AddAsync(installation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return new CreateInstallationResult
        {
            InstallationId = installation.Id,
            EnvironmentId = installation.EnvironmentId,
            Version = installation.ProductVersion,
            TaskIds = taskIds,
            CreatedAt = installation.CreatedAt
        };
    }
}
