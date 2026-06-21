namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkPetition
{
    public Guid Id { get; set; }
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public string Protocol { get; set; } = string.Empty;
    public DateOnly? FiledAt { get; set; }
    public string? ServiceCode { get; set; }
    public string? ClientName { get; set; }
    public string? Delivery { get; set; }
    public DateOnly? DeliveryDate { get; set; }
}
