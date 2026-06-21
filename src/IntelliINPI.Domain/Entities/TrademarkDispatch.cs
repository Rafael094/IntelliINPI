namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkDispatch
{
    public Guid Id { get; set; }
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? RpiNumber { get; set; }
    public DateOnly PublishedAt { get; set; }
}
