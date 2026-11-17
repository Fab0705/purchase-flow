using MediatR;

namespace backend.Modules.Logistics.Domain.Events
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}