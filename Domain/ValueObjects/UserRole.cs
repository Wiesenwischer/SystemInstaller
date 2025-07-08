namespace SystemInstaller.Domain.ValueObjects;

public record UserRole
{
    public static readonly UserRole Admin = new("Admin");
    public static readonly UserRole Customer = new("Customer");
    
    private static readonly HashSet<string> ValidRoles = new() { "Admin", "Customer" };
    
    public string Value { get; init; }
    
    public UserRole(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Role cannot be empty", nameof(value));
        
        if (!ValidRoles.Contains(value))
            throw new ArgumentException($"Invalid role: {value}. Valid roles are: {string.Join(", ", ValidRoles)}", nameof(value));
        
        Value = value;
    }
    
    public bool IsAdmin => Value == Admin.Value;
    public bool IsCustomer => Value == Customer.Value;
    
    public static implicit operator string(UserRole role) => role.Value;
    public static implicit operator UserRole(string role) => new(role);
    
    public override string ToString() => Value;
}
