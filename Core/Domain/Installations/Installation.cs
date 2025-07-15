using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Installations;

/// <summary>
/// Installation aggregate root - manages the installation lifecycle and tasks
/// </summary>
public class Installation : AggregateRoot<Guid>
{
    public Guid EnvironmentId { get; private set; }
    public string ProductVersion { get; private set; } = default!;
    public InstallationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Collections
    private readonly List<InstallationTask> _tasks = new();
    public IReadOnlyList<InstallationTask> Tasks => _tasks.AsReadOnly();

    // Navigation properties
    public InstallationEnvironment Environment { get; private set; } = default!;

    private Installation() { } // For EF Core

    public Installation(Guid environmentId, string productVersion)
    {
        Id = Guid.NewGuid();
        EnvironmentId = environmentId;
        ProductVersion = productVersion ?? throw new ArgumentNullException(nameof(productVersion));
        Status = InstallationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public InstallationTask AddTask(string name, string description, string scriptPath, string parameters = "")
    {
        if (Status != InstallationStatus.Pending)
            throw new BusinessRuleViolationException($"Cannot add tasks to installation in {Status} status");

        var order = _tasks.Count + 1;
        var task = new InstallationTask(Id, EnvironmentId, name, description, scriptPath, parameters, order);
        _tasks.Add(task);
        IncrementVersion();

        return task;
    }

    public void Start()
    {
        if (Status != InstallationStatus.Pending)
            throw new BusinessRuleViolationException($"Installation must be in pending status to start. Current status: {Status}");

        if (_tasks.Count == 0)
            throw new BusinessRuleViolationException("Cannot start installation without tasks");

        Status = InstallationStatus.Running;
        StartedAt = DateTime.UtcNow;
        ErrorMessage = null;

        AddDomainEvent(new InstallationStartedEvent(Id, EnvironmentId));
        IncrementVersion();
    }

    public void CompleteTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            throw new EntityNotFoundException(nameof(InstallationTask), taskId);

        if (task.Status != TaskStatus.Running)
            throw new BusinessRuleViolationException($"Task must be running to complete. Current status: {task.Status}");

        task.Complete();
        AddDomainEvent(new InstallationTaskCompletedEvent(Id, taskId, TaskStatus.Completed));

        CheckAndUpdateInstallationStatus();
        IncrementVersion();
    }

    public void FailTask(Guid taskId, string errorMessage)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            throw new EntityNotFoundException(nameof(InstallationTask), taskId);

        if (task.Status != TaskStatus.Running)
            throw new BusinessRuleViolationException($"Task must be running to fail. Current status: {task.Status}");

        task.Fail(errorMessage);
        AddDomainEvent(new InstallationTaskCompletedEvent(Id, taskId, TaskStatus.Failed));

        // Installation fails if any task fails
        Status = InstallationStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = $"Task '{task.Name}' failed: {errorMessage}";

        AddDomainEvent(new InstallationCompletedEvent(Id, false));
        IncrementVersion();
    }

    public void Cancel()
    {
        if (Status != InstallationStatus.Running && Status != InstallationStatus.Pending)
            throw new BusinessRuleViolationException($"Cannot cancel installation in {Status} status");

        Status = InstallationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;

        // Cancel all pending and running tasks
        foreach (var task in _tasks.Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.Running))
        {
            if (task.Status == TaskStatus.Running)
            {
                task.Fail("Installation cancelled");
            }
            else
            {
                task.Skip("Installation cancelled");
            }
        }

        AddDomainEvent(new InstallationCompletedEvent(Id, false));
        IncrementVersion();
    }

    public InstallationTask? GetNextPendingTask()
    {
        return _tasks
            .Where(t => t.Status == TaskStatus.Pending)
            .OrderBy(t => t.Order)
            .FirstOrDefault();
    }

    public int GetProgressPercentage()
    {
        if (_tasks.Count == 0) return 0;

        var completedTasks = _tasks.Count(t => t.Status == TaskStatus.Completed);
        return (int)Math.Round((double)completedTasks / _tasks.Count * 100);
    }

    private void CheckAndUpdateInstallationStatus()
    {
        if (Status != InstallationStatus.Running) return;

        var allTasksFinished = _tasks.All(t => t.IsFinished);
        if (allTasksFinished)
        {
            var hasFailedTasks = _tasks.Any(t => t.Status == TaskStatus.Failed);
            if (hasFailedTasks)
            {
                Status = InstallationStatus.Failed;
                var failedTask = _tasks.First(t => t.Status == TaskStatus.Failed);
                ErrorMessage = $"Installation failed due to task failure: {failedTask.ErrorMessage}";
            }
            else
            {
                Status = InstallationStatus.Completed;
            }

            CompletedAt = DateTime.UtcNow;
            AddDomainEvent(new InstallationCompletedEvent(Id, Status == InstallationStatus.Completed));
        }
    }
}
