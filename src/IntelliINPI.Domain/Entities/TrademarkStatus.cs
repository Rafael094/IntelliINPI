namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkStatus
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Trademark> Trademarks { get; set; } = [];
}
