using MediatR;
using SystemInstaller.Application.IntegrationEvents;
using SystemInstaller.Domain.Tenants;

namespace SystemInstaller.Application.Tenants.EventHandlers;

/// <summary>
/// Domain event handler that translates TenantCreatedEvent to integration event
/// </summary>
public class TenantCreatedDomainEventHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly IEventBus _eventBus;
    
    public TenantCreatedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Translate domain event to integration event using primitive types
        var integrationEvent = new TenantCreatedIntegrationEvent(
            TenantId: notification.TenantId.Value, // Convert strongly-typed ID to primitive
            Name: notification.TenantName,
            ContactEmail: notification.ContactEmail.Value, // Convert value object to primitive
            CreatedAt: DateTime.UtcNow
        );
        
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

/// <summary>
/// Domain event handler that translates TenantUserInvitedEvent to integration event
/// </summary>
public class TenantUserInvitedDomainEventHandler : INotificationHandler<TenantUserInvitedEvent>
{
    private readonly IEventBus _eventBus;
    
    public TenantUserInvitedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public async Task Handle(TenantUserInvitedEvent notification, CancellationToken cancellationToken)
    {
        // Translate domain event to integration event using primitive types
        var integrationEvent = new TenantUserInvitedIntegrationEvent(
            TenantId: notification.TenantId.Value, // Convert strongly-typed ID to primitive
            InvitationId: notification.InvitationId.Value, // Convert strongly-typed ID to primitive
            Email: notification.InviteeEmail.Value, // Convert value object to primitive
            FirstName: notification.InviteeName.FirstName, // Extract primitive values
            LastName: notification.InviteeName.LastName,
            Role: notification.InviteeRole.ToString(), // Convert enum to string
            InvitedAt: DateTime.UtcNow
        );
        
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

/// <summary>
/// Domain event handler that translates UserInvitationAcceptedEvent to integration event
/// </summary>
public class UserInvitationAcceptedDomainEventHandler : INotificationHandler<UserInvitationAcceptedEvent>
{
    private readonly IEventBus _eventBus;
    
    public UserInvitationAcceptedDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public async Task Handle(UserInvitationAcceptedEvent notification, CancellationToken cancellationToken)
    {
        // Translate domain event to integration event using primitive types
        var integrationEvent = new UserInvitationAcceptedIntegrationEvent(
            TenantId: notification.TenantId.Value, // Convert strongly-typed ID to primitive
            InvitationId: notification.InvitationId.Value, // Convert strongly-typed ID to primitive
            UserId: notification.UserId, // Already primitive
            Email: notification.UserEmail, // This would need to be added to the domain event
            AcceptedAt: DateTime.UtcNow
        );
        
        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

/// <summary>
/// Event bus abstraction for publishing integration events
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default);
}
