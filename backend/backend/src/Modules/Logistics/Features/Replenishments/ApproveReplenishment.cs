using backend.Common.Exceptions;
using backend.Common.Interfaces;
using backend.Modules.Logistics.Database;
using backend.Modules.Logistics.Domain.Constants;
using backend.Modules.Logistics.Features.EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Logistics.Features.Replenishments
{
    public class ApproveReplenishment
    {
        public record Command(Guid ReplenishmentId) : IRequest;
        public class Handler : IRequestHandler<Command>
        {
            private readonly LogisticDbContext _dbContext;
            private readonly ICurrentService _currentService;
            public Handler(LogisticDbContext dbContext, ICurrentService currentService)
            {
                _dbContext = dbContext;
                _currentService = currentService;
            }
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var replenishment = await _dbContext.Replenishments
                    .Include(r => r.Items)
                    .FirstOrDefaultAsync(r => r.Id == request.ReplenishmentId, cancellationToken);

                if (replenishment == null)
                    throw new NotFoundException("Solicitud de Reabastecimiento", request.ReplenishmentId);

                replenishment.Approve();

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapLogisticGroup().MapPost("/replenishments/{id:guid}/approve", async (Guid id, ISender sender) =>
                {
                    await sender.Send(new Command(id));
                    return Results.NoContent();
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = RequiredRoles.ConsultantLogistic })
                .WithName("ApproveReplenishment")
                .WithTags("Logistics - Replenishments");
            }
        }
    }
}