using backend.Common.Exceptions;
using backend.Common.Interfaces;
using backend.Modules.Logistics.Database;
using backend.Modules.Logistics.Domain.Constants;
using backend.Modules.Logistics.Features.EndpointGroup;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Logistics.Features.Replenishments
{
    public class RefuseReplenishment
    {
        public record RefuseBody(string Reason);
        public record Command(Guid ReplenishmentId, string Reason) : IRequest;
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Reason)
                    .NotEmpty().WithMessage("Debe ingresar de manera obligatoria el motivo del rechazo del pedido.");
            }
        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly LogisticDbContext _dbContext;
            private readonly ICurrentService _currentService;
            public Handler(LogisticDbContext dbcontext, ICurrentService currentService)
            {
                _dbContext = dbcontext;
                _currentService = currentService;
            }
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var replenishment = await _dbContext.Replenishments
                    .FirstOrDefaultAsync(r => r.Id == request.ReplenishmentId, cancellationToken);

                if (replenishment == null)
                    throw new NotFoundException("Solicitud de Reabastecimiento", request.ReplenishmentId);

                replenishment.Refuse(request.Reason);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapLogisticGroup().MapPost("/replnishment/{id:guid}/refuse", async (Guid id, RefuseBody body, ISender sender) =>
                {
                    await sender.Send(new Command(id, body.Reason));
                    return Results.NoContent();
                })
                .RequireAuthorization(new AuthorizeAttribute { Roles = RequiredRoles.ConsultantLogistic })
                .WithName("RefuseReplenishment")
                .WithTags("Logistics - Replenishments");
            }
        }
    }
}