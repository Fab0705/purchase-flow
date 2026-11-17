using backend.Common.Exceptions;

namespace backend.Modules.Inventory.Domain.Exceptions
{
    public class DuplicateWorkOrderItemException : DomainException
    {
        public DuplicateWorkOrderItemException(Guid sparePartId)
            : base($"El repuesto con ID '{sparePartId}' ya se encuentra listado en esta orden de trabajo.")
        {
        }
    }
}