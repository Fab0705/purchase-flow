using backend.Common.Exceptions;

namespace backend.Modules.Logistics.Domain.Exceptions
{
    public class EmptyLogisticsDocumentException : DomainException
    {
        public EmptyLogisticsDocumentException(string documentType)
            : base($"El documento logístico '{documentType}' no puede procesarse porque no contiene ítems asignados.")
        {
        }
    }
}