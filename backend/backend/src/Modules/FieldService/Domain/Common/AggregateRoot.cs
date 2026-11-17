using backend.Common.Interfaces;
using backend.Modules.FieldService.Domain.Events;

namespace backend.Modules.FieldService.Domain
{
    public abstract class AggregateRoot : IAuditableEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        // Propiedad de solo lectura para exponer los eventos acumulados
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public DateTime CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAtUtc { get; set; }
        public string? LastModifiedBy { get; set; }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}