namespace SystemInstaller.SharedKernel;

/// <summary>
/// Base class for composite identities made up of multiple values
/// </summary>
public abstract class CompositeIdentity : ValueObject
{
    /// <summary>
    /// Gets all the component values that make up this composite identity
    /// </summary>
    /// <returns>An enumerable of the component values</returns>
    protected abstract IEnumerable<object?> GetIdentityComponents();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        return GetIdentityComponents();
    }

    public override string ToString()
    {
        var components = GetIdentityComponents()
            .Where(c => c != null)
            .Select(c => c!.ToString())
            .Where(s => !string.IsNullOrEmpty(s));
        
        return string.Join("_", components);
    }
}
