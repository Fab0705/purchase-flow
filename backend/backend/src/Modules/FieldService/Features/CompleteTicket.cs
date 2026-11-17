using backend.Common.Exceptions;
using backend.Common.Interfaces;
using backend.Modules.FieldService.Database;
using backend.Modules.FieldService.Domain.Constants;
using backend.Modules.FieldService.Features.EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.Modules.FieldService.Features
{
    public static class CompleteTicket
    {
        public record Command(Guid TicketId) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            private readonly FieldServiceDbContext _dbContext;
            private readonly ICurrentService _currentService;

            public Handler(FieldServiceDbContext dbContext, ICurrentService currentService)
            {
                _dbContext = dbContext;
                _currentService = currentService;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
                if (ticket == null) throw new NotFoundException("Ticket", request.TicketId);

                // Validar que un técnico no intente cerrar el ticket de otro compañero
                if (!_currentService.IsAdmin && ticket.AssignedTechnicianId != _currentService.UserId)
                {
                    throw new UnauthorizedAccessException("Solo el técnico asignado a este ticket puede completarlo.");
                }

                ticket.Complete();
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapFieldServiceGroup().MapPost("/tickets/{id:guid}/complete", async (Guid id, ISender sender) =>
                {
                    await sender.Send(new Command(id));
                    return Results.NoContent();
                })
                .WithTags("FieldService - Tickets")
                .RequireAuthorization(new AuthorizeAttribute { Roles = RequiredRoles.Guest });
            }
        }
    }
}
