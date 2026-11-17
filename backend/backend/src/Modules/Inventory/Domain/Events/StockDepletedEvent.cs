namespace backend.Modules.Inventory.Domain.Events
{
    public record StockDepletedEvent(Guid SparePartId, Guid LocationId, int CurrentQuantity) : IDomainEvent
    {
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}