using backend.Common.Interfaces;
using backend.Modules.FieldService.Database;
using backend.Modules.FieldService.Domain.Entities;
using backend.Modules.FieldService.Features.EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.Modules.FieldService.Features
{
    public static class CreateTicket
    {
        public record Command(string Title, string Description) : IRequest<Guid>;

        public class Handler : IRequestHandler<Command, Guid>
        {
            private readonly FieldServiceDbContext _dbContext;
            private readonly ICurrentService _currentService;

            public Handler(FieldServiceDbContext dbContext, ICurrentService currentService)
            {
                _dbContext = dbContext;
                _currentService = currentService;
            }

            public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
            {
                // El ticket se asocia automáticamente a la provincia del usuario que lo crea
                var ticket = new Ticket(Guid.NewGuid(), request.Title, request.Description, _currentService.LocationId);

                _dbContext.Tickets.Add(ticket);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return ticket.Id;
            }
        }

        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapFieldServiceGroup().MapPost("/tickets", async (Command command, ISender sender) =>
                {
                    var id = await sender.Send(command);
                    return Results.Created($"/api/v1/field-service/tickets/{id}", new { id });
                })
                .WithTags("FieldService - Tickets")
                .RequireAuthorization();
            }
        }
    }
}
