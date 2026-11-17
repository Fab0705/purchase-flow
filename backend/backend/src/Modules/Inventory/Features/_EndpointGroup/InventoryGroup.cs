using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace backend.Modules.Inventory.Features._EndpointGroup
{
    public static class InventoryGroup
    {
        public static RouteGroupBuilder MapInventoryGroup(this IEndpointRouteBuilder routes)
        {
            return routes.MapGroup("/api/inventory");
        }
    }
}