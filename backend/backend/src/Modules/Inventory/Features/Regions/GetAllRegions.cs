using backend.Common.Interfaces;
using backend.Modules.Inventory.Database;
using backend.Modules.Inventory.Features._EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Inventory.Features.Regions
{
    public static class GetAllRegions
    {
        public record RegionsResponse(Guid Id, string Name, string Code);
        public record Query : IRequest<List<RegionsResponse>>;
        public class Handler : IRequestHandler<Query, List<RegionsResponse>>
        {
            private readonly InventoryDbContext _dbContext;
            public Handler(InventoryDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<List<RegionsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _dbContext.Regions
                    .AsNoTracking()
                    .OrderBy(r => r.Name)
                    .Select(r => new RegionsResponse(r.Id, r.Name, r.Code))
                    .ToListAsync(cancellationToken);
            }
        }
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapInventoryGroup().MapGet("/regions", async (ISender sender) =>
                {
                    var result = await sender.Send(new Query());
                    return Results.Ok(result);
                })
                .WithName("GetAllRegions")
                .WithTags("Inventory - Regions")
                .AllowAnonymous();
            }
        }
    }
}