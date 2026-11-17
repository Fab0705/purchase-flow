using MediatR;

namespace backend.Modules.Inventory.Domain.Events
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}