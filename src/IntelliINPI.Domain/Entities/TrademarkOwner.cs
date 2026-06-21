namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkOwner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Document { get; set; }
    public ICollection<Trademark> Trademarks { get; set; } = [];
    public ICollection<TrademarkOwnerLink> TrademarkLinks { get; set; } = [];
}
