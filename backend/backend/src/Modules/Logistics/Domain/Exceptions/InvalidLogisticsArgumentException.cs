using backend.Common.Exceptions;

namespace backend.Modules.Logistics.Domain.Exceptions
{
    public class InvalidLogisticsArgumentException : DomainException
    {
        public InvalidLogisticsArgumentException(string message) : base(message)
        {
        }
    }
}