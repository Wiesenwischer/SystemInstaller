using SystemInstaller.SharedKernel;

namespace SystemInstaller.Domain.Users.Model;

/// <summary>
/// Email verification token value object
/// </summary>
public class EmailVerificationToken : ValueObject
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }
    public DateTime CreatedAt { get; }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsExpired;
    
    public EmailVerificationToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));
        
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static EmailVerificationToken Generate(TimeSpan? validFor = null)
    {
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.Add(validFor ?? TimeSpan.FromHours(24));
        return new EmailVerificationToken(token, expiresAt);
    }
    
    private static string GenerateSecureToken()
    {
        // Generate a cryptographically secure random token
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Token;
        yield return ExpiresAt;
    }
}
