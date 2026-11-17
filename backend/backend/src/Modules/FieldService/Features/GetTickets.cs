using backend.Common.Interfaces;
using backend.Modules.FieldService.Database;
using backend.Modules.FieldService.Domain.Constants;
using backend.Modules.FieldService.Domain.Enums;
using backend.Modules.FieldService.Features.EndpointGroup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.FieldService.Features
{
    public static class GetTickets
    {
        // 1. DTO de Respuesta (aplanado para que sea fácil de consumir en React)
        public record TicketResponse(
            Guid Id,
            string Title,
            string Description,
            string Status,
            Guid? AssignedTechnicianId,
            DateTime? ScheduledDateUtc,
            DateTime CreatedAtUtc
        );

        // 2. Query (Parámetros de búsqueda por URL)
        public record Query(string? Status = null) : IRequest<List<TicketResponse>>;

        // 3. El Manejador (Handler)
        public class Handler : IRequestHandler<Query, List<TicketResponse>>
        {
            private readonly FieldServiceDbContext _dbContext;
            private readonly ICurrentService _currentService;

            public Handler(FieldServiceDbContext dbContext, ICurrentService currentService)
            {
                _dbContext = dbContext;
                _currentService = currentService;
            }

            public async Task<List<TicketResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Iniciamos la consulta base filtrando por la provincia/ubicación actual del usuario
                var query = _dbContext.Tickets
                    .AsNoTracking()
                    .Where(t => t.LocationId == _currentService.LocationId);

                // 🛡️ REGLA DE NEGOCIO: Si NO es administrador, solo puede ver SUS propios tickets
                if (!_currentService.IsAdmin)
                {
                    query = query.Where(t => t.AssignedTechnicianId == _currentService.UserId);
                }

                // 🔍 FILTRADO DINÁMICO: Si mandaron un estado por la URL, lo aplicamos
                if (!string.IsNullOrWhiteSpace(request.Status) &&
                    Enum.TryParse<TicketStatus>(request.Status, true, out var parsedStatus))
                {
                    query = query.Where(t => t.Status == parsedStatus);
                }

                // Proyectamos hacia nuestro DTO de respuesta
                var tickets = await query
                    .OrderByDescending(t => t.CreatedAtUtc)
                    .Select(t => new TicketResponse(
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Status.ToString(), // Convertimos el Enum a texto (Open, InProgress, etc.)
                        t.AssignedTechnicianId,
                        t.ScheduledDateUtc,
                        t.CreatedAtUtc
                    ))
                    .ToListAsync(cancellationToken);

                return tickets;
            }
        }

        // 4. El Endpoint Minimal API
        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapFieldServiceGroup().MapGet("/tickets", async ([AsParameters] Query query, ISender sender) =>
                {
                    var result = await sender.Send(query);
                    return Results.Ok(result);
                })
                .WithTags("FieldService - Tickets")
                .RequireAuthorization(new AuthorizeAttribute { Roles = $"{RequiredRoles.OperatorManager}, {RequiredRoles.Guest}" })
                .WithName("GetTickets");
            }
        }
    }
}