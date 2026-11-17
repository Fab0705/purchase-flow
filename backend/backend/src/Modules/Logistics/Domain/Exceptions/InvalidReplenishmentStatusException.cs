using backend.Common.Exceptions;

namespace backend.Modules.Logistics.Domain.Exceptions
{
    public class InvalidReplenishmentStatusException : DomainException
    {
        public InvalidReplenishmentStatusException(string currentStatus, string intendedAction)
            : base($"No se puede realizar la acción '{intendedAction}' debido a que la solicitud de reabastecimiento está en estado '{currentStatus}'.")
        {
        }
    }
}