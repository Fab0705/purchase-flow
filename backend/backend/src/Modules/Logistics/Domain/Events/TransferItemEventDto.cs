namespace backend.Modules.Logistics.Domain.Events
{
    public record TransferItemEventDto(Guid SparePartId, int Quantity);
}