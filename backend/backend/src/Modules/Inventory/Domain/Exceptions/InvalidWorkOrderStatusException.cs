using backend.Common.Exceptions;

namespace backend.Modules.Inventory.Domain.Exceptions
{
    public class InvalidWorkOrderStatusException : DomainException
    {
        public InvalidWorkOrderStatusException(string currentStatus, string intendedAction)
            : base($"No se puede realizar la acción '{intendedAction}' debido a que la orden de trabajo se encuentra en estado '{currentStatus}'.")
        {
        }
    }
}