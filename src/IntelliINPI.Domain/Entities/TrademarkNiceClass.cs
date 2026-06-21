namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkNiceClass
{
    public Guid Id { get; set; }
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public int ClassNumber { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Specification { get; set; }
}
