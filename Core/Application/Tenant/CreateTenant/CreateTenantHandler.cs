using MediatR;
using SystemInstaller.Core.Domain.Tenant;
using SystemInstaller.SharedKernel;

namespace SystemInstaller.Core.Application.Tenant.CreateTenant;

public class CreateTenantCommand : IRequest<CreateTenantResult>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = default!;
}

public class CreateTenantResult
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, CreateTenantResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTenantResult> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Check if tenant already exists
        if (await _tenantRepository.ExistsAsync(request.Name, cancellationToken))
        {
            throw new BusinessRuleViolationException($"Tenant with name '{request.Name}' already exists");
        }

        // Create domain entity
        var tenant = new Tenant(request.Name, request.Description, new Email(request.ContactEmail));

        // Save to repository
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return new CreateTenantResult
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            ContactEmail = tenant.ContactEmail,
            CreatedAt = tenant.CreatedAt
        };
    }
}
