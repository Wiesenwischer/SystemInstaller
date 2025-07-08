namespace SystemInstaller.Domain.ValueObjects;

public record UserRole
{
    private static readonly HashSet<string> ValidRoles = new() { "Admin", "Customer", "Member", "Developer" };
    
    public static readonly UserRole Admin = new("Admin");
    public static readonly UserRole Customer = new("Customer");
    public static readonly UserRole Member = new("Member");
    public static readonly UserRole Developer = new("Developer");
    
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
    public bool IsMember => Value == Member.Value;
    public bool IsDeveloper => Value == Developer.Value;
    
    public static implicit operator string(UserRole role) => role.Value;
    public static implicit operator UserRole(string role) => new(role);
    
    public override string ToString() => Value;
}
