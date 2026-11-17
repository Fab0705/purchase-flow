using backend.Common.Contracts;
using backend.Modules.Inventory.Domain.Events;
using MediatR;

namespace backend.Modules.Inventory.Features.StockManagement.EventHandlers
{
    public class StockDepletedHandler : INotificationHandler<StockDepletedEvent>
    {
        private readonly IPublisher _publisher;
        public StockDepletedHandler(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Handle(StockDepletedEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = new StockDepletedIntegrationEvent(
                notification.SparePartId,
                notification.LocationId,
                notification.CurrentQuantity
            );

            await _publisher.Publish(integrationEvent, cancellationToken);
        }
    }
}
