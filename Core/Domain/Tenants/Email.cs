using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Tenants;

/// <summary>
/// Represents an email address value object
/// </summary>
public class Email : ValueObject
{
    public string Value { get; private set; } = default!;

    private Email() { } // For EF Core

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}
