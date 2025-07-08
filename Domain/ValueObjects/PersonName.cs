namespace SystemInstaller.Domain.ValueObjects;

public record PersonName
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    
    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        
        if (firstName.Length > 50)
            throw new ArgumentException("First name cannot exceed 50 characters", nameof(firstName));
        
        if (lastName.Length > 50)
            throw new ArgumentException("Last name cannot exceed 50 characters", nameof(lastName));
        
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
    
    public string FullName => $"{FirstName} {LastName}";
    
    public override string ToString() => FullName;
}
