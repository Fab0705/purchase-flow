using MediatR;

namespace backend.Modules.FieldService.Domain.Events
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}