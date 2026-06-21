namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkOwnerLink
{
    public Guid Id { get; set; }
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public TrademarkOwner Owner { get; set; } = null!;
}
