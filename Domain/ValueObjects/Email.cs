using System.Text.RegularExpressions;

namespace SystemInstaller.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);
    
    public string Value { get; init; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        
        if (value.Length > 100)
            throw new ArgumentException("Email cannot exceed 100 characters", nameof(value));
        
        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Invalid email format: '{value}'", nameof(value));
        
        Value = value.ToLowerInvariant();
    }
    
    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
    
    public override string ToString() => Value;
}
