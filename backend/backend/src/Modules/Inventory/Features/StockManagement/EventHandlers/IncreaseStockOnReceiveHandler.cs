using backend.Common.Contracts;
using backend.Modules.Inventory.Database;
using backend.Modules.Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Inventory.Features.StockManagement.EventHandlers
{
    public class IncreaseStockOnReceiveHandler : INotificationHandler<TransferReceivedIntegrationEvent>
    {
        private readonly InventoryDbContext _dbContext;

        public IncreaseStockOnReceiveHandler(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(TransferReceivedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            foreach (var item in notification.Items)
            {
                var stock = await _dbContext.Stocks
                    .FirstOrDefaultAsync(s => s.LocationId == notification.DestinationLocationId && s.SparePartId == item.SparePartId, cancellationToken);

                if (stock != null)
                {
                    stock.Increment(item.Quantity);
                }
                else
                {
                    var newStock = new Stock(Guid.NewGuid(), item.SparePartId, notification.DestinationLocationId, item.Quantity);
                    _dbContext.Stocks.Add(newStock);
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}