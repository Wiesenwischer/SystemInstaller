using FluentValidation;

namespace SystemInstaller.SharedKernel;

/// <summary>
/// Base class for specifications (specification pattern)
/// </summary>
/// <typeparam name="T">The type to validate</typeparam>
public abstract class Specification<T>
{
    /// <summary>
    /// Checks if the specification is satisfied
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if satisfied, false otherwise</returns>
    public abstract bool IsSatisfiedBy(T item);

    /// <summary>
    /// Combines this specification with another using AND logic
    /// </summary>
    /// <param name="other">The other specification</param>
    /// <returns>A new specification that is satisfied when both are satisfied</returns>
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// Combines this specification with another using OR logic
    /// </summary>
    /// <param name="other">The other specification</param>
    /// <returns>A new specification that is satisfied when either is satisfied</returns>
    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// Negates this specification
    /// </summary>
    /// <returns>A new specification that is satisfied when this is not satisfied</returns>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T item)
    {
        return _left.IsSatisfiedBy(item) && _right.IsSatisfiedBy(item);
    }
}

internal class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T item)
    {
        return _left.IsSatisfiedBy(item) || _right.IsSatisfiedBy(item);
    }
}

internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override bool IsSatisfiedBy(T item)
    {
        return !_specification.IsSatisfiedBy(item);
    }
}
