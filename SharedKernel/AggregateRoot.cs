namespace SystemInstaller.SharedKernel;

/// <summary>
/// Base class for aggregate roots
/// </summary>
/// <typeparam name="TId">The type of the identifier</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Version for optimistic concurrency control
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// Increments the version for optimistic concurrency
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}
