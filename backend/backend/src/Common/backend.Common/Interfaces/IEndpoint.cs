using Microsoft.AspNetCore.Routing;

namespace backend.Common.Interfaces
{
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);
    }
}