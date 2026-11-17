using backend.Common.Exceptions;
using backend.Modules.FieldService.Domain.Enums;
using backend.Modules.FieldService.Domain.Exceptions;

namespace backend.Modules.FieldService.Domain.Entities
{
    public class Ticket : AggregateRoot
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public Guid LocationId { get; private set; }

        // El técnico que hará el trabajo (Guid del módulo Identity)
        public Guid? AssignedTechnicianId { get; private set; }
        public TicketStatus Status { get; private set; }
        public DateTime? ScheduledDateUtc { get; private set; }

        private Ticket() { }

        public Ticket(Guid id, string title, string description, Guid locationId)
        {
            if (id == Guid.Empty) throw new ArgumentException("El ID del ticket es obligatorio.");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título es obligatorio.");

            Id = id;
            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            LocationId = locationId;
            Status = TicketStatus.Open;
        }

        public void AssignTechnician(Guid technicianId, DateTime scheduledDateUtc)
        {
            if (Status != TicketStatus.Open && Status != TicketStatus.Assigned)
                throw new InvalidDomainArgumentException($"No se puede asignar un técnico a un ticket en estado {Status}.");

            if (technicianId == Guid.Empty)
                throw new InvalidDomainArgumentException("El ID del técnico es inválido.");

            AssignedTechnicianId = technicianId;
            ScheduledDateUtc = scheduledDateUtc;
            Status = TicketStatus.Assigned;
        }

        public void Complete()
        {
            if (Status != TicketStatus.Assigned && Status != TicketStatus.InProgress)
                throw new InvalidDomainArgumentException("Solo se pueden completar tickets que hayan sido asignados o estén en progreso.");

            Status = TicketStatus.Completed;
        }
    }
}