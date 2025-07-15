using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Domain.TenantManagement;

/// <summary>
/// Represents a person's name value object
/// </summary>
public class PersonName : ValueObject
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    private PersonName() { } // For EF Core

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
