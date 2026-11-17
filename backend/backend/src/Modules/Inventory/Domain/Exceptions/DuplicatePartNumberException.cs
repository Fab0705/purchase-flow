using backend.Common.Exceptions;

namespace backend.Modules.Inventory.Domain.Exceptions
{
    public class DuplicatePartNumberException : DomainException
    {
        public DuplicatePartNumberException(string partNumber)
            : base($"El número de parte '{partNumber}' ya se encuentra registrado en el catálogo de repuestos.")
        {
        }
    }
}