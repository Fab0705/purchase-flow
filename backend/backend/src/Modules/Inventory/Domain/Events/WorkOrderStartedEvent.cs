using backend.Modules.Inventory.Domain.Entities;

namespace backend.Modules.Inventory.Domain.Events
{
    public record WorkOrderStartedEvent(Guid WorkOrderId, Guid LocationId, IReadOnlyCollection<WorkOrderItem> Items) : IDomainEvent
    {
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}