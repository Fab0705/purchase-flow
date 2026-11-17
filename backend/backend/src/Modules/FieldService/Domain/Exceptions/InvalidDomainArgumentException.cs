using backend.Common.Exceptions;

namespace backend.Modules.FieldService.Domain.Exceptions
{
    public class InvalidDomainArgumentException : DomainException
    {
        public InvalidDomainArgumentException(string message) : base(message)
        {
        }
    }
}