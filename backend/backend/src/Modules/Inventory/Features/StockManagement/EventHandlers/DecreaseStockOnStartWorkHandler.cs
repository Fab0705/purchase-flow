using backend.Common.Contracts;
using backend.Modules.Inventory.Database;
using backend.Modules.Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Inventory.Features.StockManagement.EventHandlers
{
    public class DecreaseStockOnStartWorkHandler : INotificationHandler<WorkOrderStartWorkIntegrationEvent>
    {
        private readonly InventoryDbContext _dbContext;
        public DecreaseStockOnStartWorkHandler(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Handle(WorkOrderStartWorkIntegrationEvent notification, CancellationToken cancellationToken)
        {
            foreach (var item in notification.Items)
            {
                var stock = await _dbContext.Stocks
                            .FirstOrDefaultAsync(s => s.LocationId == notification.LocationId
                                           && s.SparePartId == item.SparePartId, cancellationToken);
                if (stock != null)
                {
                    stock.Decrement(item.Quantity);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}