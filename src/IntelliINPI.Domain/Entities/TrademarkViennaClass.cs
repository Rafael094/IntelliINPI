namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkViennaClass
{
    public Guid Id { get; set; }
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public string Edition { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}
