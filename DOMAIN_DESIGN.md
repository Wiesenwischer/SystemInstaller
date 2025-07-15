# SystemInstaller Domain Design

## Overview

This document outlines the domain design principles and patterns used in the SystemInstaller system. We follow Domain-Driven Design (DDD) principles with Clean Architecture patterns.

## Domain Structure

### Aggregate Organization

Our domain is organized by **aggregates** rather than technical classifications:

```
Core/Domain/
├── Tenants/          # Tenant aggregate and related entities
├── Installations/    # Installation aggregate and related entities
└── ...               # Other aggregates as they emerge
```

### Namespace Structure

- **Domain Layer**: `SystemInstaller.Domain.{Aggregate}`
- **Application Layer**: `SystemInstaller.Application.{Aggregate}`
- **Shared Kernel**: `SystemInstaller.SharedKernel`

## Identity Design Principles

### Strongly-Typed IDs

We use strongly-typed identities instead of primitive types (Guid, int, etc.) to:

1. **Prevent primitive obsession**
2. **Provide type safety** - can't accidentally pass TenantId where InstallationId is expected
3. **Express domain concepts clearly**
4. **Enable better IDE support and refactoring**

Example:
```csharp
// Instead of this:
public void AssignToTenant(Guid tenantId, Guid userId) 

// We use this:
public void AssignToTenant(TenantId tenantId, UserId userId)
```

### Composite Identity Design Heuristics

When designing composite identities, follow these heuristics:

#### 1. Does the pair itself form the identity of the aggregate?
If **yes** → Create a composite ID value object.

Example: A `TenantUserKey` if the combination of TenantId + UserId forms a unique aggregate identity.

```csharp
public class TenantUserKey : CompositeIdentity
{
    public TenantId TenantId { get; }
    public UserId UserId { get; }
    
    protected override IEnumerable<object?> GetIdentityComponents()
    {
        yield return TenantId;
        yield return UserId;
    }
}
```

#### 2. Is one part already the aggregate root's ID?
Then often you only need the root ID in child entities.

Example: Inside the `Tenant` aggregate, `TenantUser` entities might store only `UserId` because the aggregate root (`Tenant`) already carries `TenantId`.

```csharp
public class Tenant : AggregateRoot<TenantId>
{
    private readonly List<TenantUser> _tenantUsers = new();
    
    // TenantUser only needs UserId, not TenantId
    public void AddUser(UserId userId, Email email, PersonName name) { ... }
}
```

#### 3. Avoid leaking internals
Only expose composite IDs where they truly matter at the boundary:
- Repository keys
- External contracts
- Cross-aggregate references

## Base Classes

### Identity<TValue>
Base class for strongly-typed single-value identities.

```csharp
public class TenantId : Identity<Guid>
{
    public TenantId(Guid value) : base(value) { }
    public static TenantId New() => new(Guid.NewGuid());
}
```

### CompositeIdentity
Base class for multi-value composite identities.

```csharp
public class OrderLineKey : CompositeIdentity
{
    public OrderId OrderId { get; }
    public int LineNumber { get; }
    
    protected override IEnumerable<object?> GetIdentityComponents()
    {
        yield return OrderId;
        yield return LineNumber;
    }
}
```

### Entity<TId>
Base class for domain entities with strongly-typed identities.

```csharp
public class Tenant : Entity<TenantId>
{
    // Domain logic here
}
```

### AggregateRoot<TId>
Base class for aggregate roots with versioning and domain events.

```csharp
public class Tenant : AggregateRoot<TenantId>
{
    // Aggregate root specific logic
}
```

## Aggregate Design Principles

### 1. Tenant Aggregate
**Responsibility**: Manages tenant lifecycle and user management within the tenant context.

**Entities**:
- `Tenant` (Root)
- `TenantUser` 
- `UserInvitation`

**Value Objects**:
- `TenantId`
- `TenantUserId`
- `UserInvitationId`
- `Email`
- `PersonName`

**Business Rules**:
- Cannot invite user if already exists in tenant
- Cannot have multiple pending invitations for same email
- Only active tenants can invite users

### 2. Installation Aggregate
**Responsibility**: Manages installation lifecycle and task execution.

**Entities**:
- `Installation` (Root)
- `InstallationTask`
- `InstallationEnvironment`

**Value Objects**:
- `InstallationId`
- `InstallationTaskId`
- `InstallationEnvironmentId`

**Business Rules**:
- Installation tasks must be executed in sequence
- Cannot start installation if environment is not ready
- Installation status must progress through valid state transitions

## Repository Patterns

Repositories are defined within the aggregate namespace and use strongly-typed IDs:

```csharp
public interface ITenantRepository : IRepository<Tenant, TenantId>
{
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

## Domain Events

Domain events are raised by aggregates when significant business events occur:

```csharp
public class TenantCreatedEvent : DomainEvent
{
    public TenantId TenantId { get; }
    public string Name { get; }
    public Email ContactEmail { get; }
}
```

### Domain Events vs Integration Events

#### ✅ Domain Events (Internal to Domain Model)
Use **strongly-typed domain IDs** in domain events:

```csharp
public record TenantUserInvited(TenantId TenantId, UserInvitationId InvitationId, string Email);
```

**Benefits:**
- **Type safety** - Cannot mix up IDs or forget conversions
- **Ubiquitous language** - Events clearly reflect your domain model
- **Consistency** - Keeps domain language aligned across aggregates and events
- **No leaky abstractions** - Domain doesn't expose internals as primitives

#### ⚠️ Integration Events (External Contracts)
When events leave your bounded context (e.g., via message bus), use **primitive types**:

```csharp
public record TenantUserInvitedIntegrationEvent(Guid TenantId, Guid UserId, string Email);
```

**Reasons:**
- **Simple serialization** - No custom converters needed
- **Loose coupling** - Other contexts don't depend on our ID types
- **Portability** - Can be consumed by any technology stack

#### 🧠 Summary Table

| Event Type | ID Type in Event | Reason |
|------------|------------------|---------|
| Domain Event | `TenantId`, `UserId`, etc. | Keeps domain pure and type-safe |
| Integration Event | `Guid`, `string`, etc. | Simple, portable, decoupled |

### Cross-Context ID Semantics

An important principle: **An ID is only the identity of an aggregate within its own bounded context.**

#### Same ID, Different Meaning

**In Tenant Management Context:**
```csharp
public class Tenant : AggregateRoot<TenantId>
{
    // TenantId is the IDENTITY of this aggregate
    public TenantId Id { get; }
    public string Name { get; }
    // Rich behavior, invariants apply
}
```

**In Invoicing Context:**
```csharp
public class Invoice : AggregateRoot<InvoiceId>
{
    public InvoiceId Id { get; }
    public TenantId TenantId { get; } // Just a REFERENCE, not an identity
    public Money Amount { get; }
}

// In this context, TenantId is a VALUE OBJECT
public class TenantId : ValueObject
{
    public Guid Value { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### Context Translation

**Domain event in Tenant context:**
```csharp
public record TenantCreated(TenantId Id, string Name);
```

**Integration event to other contexts:**
```csharp
public record TenantCreatedIntegrationEvent(Guid TenantId, string Name);
```

**Key Rules:**
- ✅ **In originating context**: `TenantId` is an identity
- ✅ **In other contexts**: `TenantId` is a value object used as reference
- ❌ **Don't share**: Identity base classes across bounded contexts
- ❌ **Avoid coupling**: Contexts depending on each other's internal ID types
```

## EF Core Mapping Considerations

### ID Conversions
Strongly-typed IDs require custom converters for EF Core:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Convert TenantId to/from Guid
    modelBuilder.Entity<Tenant>()
        .Property(t => t.Id)
        .HasConversion(
            id => id.Value,
            value => new TenantId(value));
}
```

### Value Object Mapping
Value objects are mapped using owned entity types:

```csharp
modelBuilder.Entity<Tenant>()
    .OwnsOne(t => t.ContactEmail, email =>
    {
        email.Property(e => e.Value).HasColumnName("ContactEmail");
    });
```

## Integration Patterns

### Event Translation Strategy

When domain events need to be published to other bounded contexts, use a translation pattern:

```csharp
// Domain event handler that translates to integration event
public class TenantCreatedDomainEventHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly IEventBus _eventBus;
    
    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Translate domain event to integration event
        var integrationEvent = new TenantCreatedIntegrationEvent(
            TenantId: notification.TenantId.Value, // Convert to primitive
            Name: notification.Name,
            ContactEmail: notification.ContactEmail.Value
        );
        
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
```

### Cross-Context References

When referencing entities from other bounded contexts:

```csharp
// In Installation context, referencing tenant
public class Installation : AggregateRoot<InstallationId>
{
    // This is a VALUE OBJECT reference, not an identity
    public TenantId TenantId { get; private set; }
    
    // We don't have access to the Tenant aggregate here
    // We only know about the TenantId as a foreign reference
}

// The TenantId in Installation context is a different class
// than the one in Tenant context
public class TenantId : ValueObject
{
    public Guid Value { get; }
    
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Invalid tenant ID.");
        Value = value;
    }
}
```

### Shared Kernel vs Context-Specific Types

**Shared Kernel (SystemInstaller.SharedKernel):**
- Base classes (`Entity<T>`, `ValueObject`, `AggregateRoot<T>`)
- Common interfaces (`IRepository<T,TId>`, `IUnitOfWork`)
- Domain exceptions
- NOT specific ID types

**Context-Specific:**
- Domain aggregates and entities
- Specific ID types (even if they wrap the same primitive)
- Business rules and invariants
- Repository implementations

**Best Practices:**
1. **Keep shared kernel minimal** - Only truly shared concepts
2. **Duplicate when in doubt** - Better than tight coupling
## Best Practices

### Domain Design
1. **Keep aggregates small** - Focus on business invariants
2. **Use strongly-typed IDs** - Avoid primitive obsession
3. **Apply composite ID heuristics** - Only when truly needed
4. **Encapsulate business rules** - Within the aggregate
5. **Raise domain events** - For significant business events
6. **Use value objects** - For concepts without identity
7. **Follow ubiquitous language** - Use domain terms consistently

### Event Design
8. **Domain events use domain types** - `TenantId`, `UserId`, etc.
9. **Integration events use primitives** - `Guid`, `string`, etc.
10. **Translate at boundaries** - Domain → Integration events
11. **Keep contexts decoupled** - Don't share specific ID types

### Cross-Context Design
12. **IDs are context-specific** - Same ID, different meaning
13. **In origin context** - ID is identity
14. **In other contexts** - ID is value object reference
15. **Avoid shared ID types** - Between bounded contexts
16. **Use primitive types** - For integration contracts

### Implementation
17. **Create EF Core converters** - For strongly-typed IDs
18. **Map value objects properly** - Using owned entity types
19. **Use implicit operators** - For convenient conversions
20. **Document aggregate boundaries** - And their responsibilities

## Migration Strategy

When introducing strongly-typed IDs to existing code:

1. Create the ID value objects
2. Update entity base classes
3. Update repository interfaces
4. Add EF Core conversions
5. Update application services
6. Update API contracts (if needed)

This ensures type safety while maintaining database compatibility.
