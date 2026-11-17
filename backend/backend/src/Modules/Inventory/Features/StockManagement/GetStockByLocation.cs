using backend.Common.Interfaces;
using backend.Modules.Inventory.Database;
using backend.Modules.Inventory.Domain.Constants;
using backend.Modules.Inventory.Features._EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Inventory.Features.StockManagement
{
    public static class GetStockByLocation
    {
        public record Query(Guid LocationId) : IRequest<List<StockResponse>>;
        public record StockResponse(Guid SparePartId, string PartNumber, int Quantity);
        public class Handler : IRequestHandler<Query, List<StockResponse>>
        {
            private readonly InventoryDbContext _dbContext;

            public Handler(InventoryDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<StockResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var stockList = await _dbContext.Stocks
                    .AsNoTracking()
                    .Where(s => s.LocationId == request.LocationId)
                    .Select(s => new StockResponse(s.SparePartId, s.SparePart.PartNumber, s.Quantity))
                    .ToListAsync(cancellationToken);

                return stockList;
            }
        }

        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapInventoryGroup().MapGet("/stock/location/{locationId:guid}", async (Guid locationId, ISender sender) =>
                {
                    var query = new Query(locationId);
                    var result = await sender.Send(query);
                    return Results.Ok(result);
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = $"{RequiredRoles.OperatorManager},{RequiredRoles.ConsultantLogistic}" })
                .WithName("GetStockByLocation")
                .WithTags("Inventory - Stock Management");
            }
        }
    }
}