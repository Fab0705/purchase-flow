using backend.Common.Exceptions;

namespace backend.Modules.Inventory.Domain.Exceptions
{
    public class EmptyWorkOrderException : DomainException
    {
        public EmptyWorkOrderException()
            : base("La orden de trabajo no puede procesarse ni cambiar de estado porque no contiene ítems asignados.")
        {
        }
    }
}