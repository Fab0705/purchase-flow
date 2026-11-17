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
    public class RegisterTransferException
    {
        public record Command(Guid TransferId, string Notes) : IRequest;
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
                var transfer = await _dbContext.Transfers
                    .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);

                if (transfer == null)
                    throw new NotFoundException("Guía de Traslado", request.TransferId);

                transfer.RegisterException(request.Notes);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapLogisticGroup().MapPost("/transfers/{transferId:Guid}/exception", async (Guid transferId, [AsParameters] Command body, ISender sender) =>
                {
                    var command = new Command(transferId, body.Notes);
                    await sender.Send(command);
                    return Results.NoContent();
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = RequiredRoles.OperatorManager })
                .WithName("RegisterTransferException")
                .WithTags("Logistics - Transfers");
            }
        }
    }
}