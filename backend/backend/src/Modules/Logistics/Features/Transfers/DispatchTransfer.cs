using backend.Common.Contracts;
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

namespace backend.Modules.Logistics.Features.Transfers
{
    public class DispatchTransfer
    {
        public record Command(Guid TransferId) : IRequest;
        public class Handler : IRequestHandler<Command>
        {
            private readonly LogisticDbContext _dbContext;
            private readonly ICurrentService _currentService;
            private readonly IPublisher _publisher;
            public Handler(LogisticDbContext dbContext, ICurrentService currentService, IPublisher publisher)
            {
                _dbContext = dbContext;
                _currentService = currentService;
                _publisher = publisher;
            }
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var transfer = await _dbContext.Transfers
                    .Include(t => t.Items)
                    .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

                if (transfer == null)
                    throw new NotFoundException("Guía de Traslado", request.TransferId);

                if (!_currentService.IsAdmin && transfer.OriginLocationId != _currentService.LocationId)
                    throw new UnauthorizedAccessException("Solo el almacén provincial de origen o un Administrador pueden autorizar la salida de este traslado.");

                transfer.Dispatch();

                await _dbContext.SaveChangesAsync(cancellationToken);

                var contractItems = transfer.Items
                            .Select(i => new TransferItemContractDto(i.SparePartId, i.Quantity))
                            .ToList();

                await _publisher.Publish(new TransferDispatchedIntegrationEvent(
                    transfer.Id,
                    transfer.TrackingCode,
                    transfer.OriginLocationId,
                    contractItems
                ), cancellationToken);
            }
        }
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapLogisticGroup().MapPost("/transfers/{transferId:guid}/dispatch", async (Guid transferId, ISender sender) =>
                {
                    await sender.Send(new Command(transferId));
                    return Results.NoContent();
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = RequiredRoles.OperatorManager })
                .WithName("Dispatch Transfer")
                .WithTags("Logistics - Transfers");
            }
        }
    }
}