using backend.Common.Exceptions;

namespace backend.Modules.Logistics.Domain.Exceptions
{
    public class SameOriginAndDestinationException : DomainException
    {
        public SameOriginAndDestinationException(Guid locationId)
            : base($"El almacén de origen y destino no pueden ser el mismo ({locationId}).")
        {
        }
    }
}