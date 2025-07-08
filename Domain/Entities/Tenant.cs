using SystemInstaller.Domain.ValueObjects;

namespace SystemInstaller.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Email ContactEmail { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    private readonly List<TenantUser> _tenantUsers = new();
    private readonly List<InstallationEnvironment> _environments = new();
    
    public IReadOnlyList<TenantUser> TenantUsers => _tenantUsers.AsReadOnly();
    public IReadOnlyList<InstallationEnvironment> Environments => _environments.AsReadOnly();
    
    private Tenant() { } // EF Core
    
    public Tenant(string name, Email contactEmail, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Tenant name cannot exceed 100 characters", nameof(name));
        
        Id = Guid.NewGuid();
        Name = name;
        ContactEmail = contactEmail;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Tenant name cannot exceed 100 characters", nameof(name));
        
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateDescription(string? description)
    {
        if (description?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));
        
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateContactEmail(Email contactEmail)
    {
        ContactEmail = contactEmail;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddUser(TenantUser user)
    {
        if (_tenantUsers.Any(u => u.UserId == user.UserId))
            throw new InvalidOperationException("User is already a member of this tenant");
        
        _tenantUsers.Add(user);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveUser(string userId)
    {
        var user = _tenantUsers.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            _tenantUsers.Remove(user);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void AddEnvironment(InstallationEnvironment environment)
    {
        _environments.Add(environment);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveEnvironment(Guid environmentId)
    {
        var environment = _environments.FirstOrDefault(e => e.Id == environmentId);
        if (environment != null)
        {
            _environments.Remove(environment);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
