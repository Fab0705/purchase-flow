using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace backend.Modules.Identity.Features._EndpointGroup
{
    public static class IdentityGroup
    {
        public static RouteGroupBuilder MapIdentityGroup(this IEndpointRouteBuilder routes)
        {
            return routes.MapGroup("/api/identity").WithTags("Identity");
        }
    }
}