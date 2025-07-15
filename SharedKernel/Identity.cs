namespace SystemInstaller.SharedKernel;

/// <summary>
/// Base class for strongly-typed identities
/// </summary>
/// <typeparam name="TValue">The underlying value type</typeparam>
public abstract class Identity<TValue> : ValueObject, IEquatable<Identity<TValue>>
    where TValue : notnull
{
    public TValue Value { get; }

    protected Identity(TValue value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public bool Equals(Identity<TValue>? other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }

    public static implicit operator TValue(Identity<TValue> identity)
    {
        return identity.Value;
    }
}
