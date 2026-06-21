namespace IntelliINPI.Domain.Entities;

public sealed class RoyaltyPayment
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public TechnologyTransferContract Contract { get; set; } = null!;
    public DateOnly Competence { get; set; }
    public decimal AmountReceived { get; set; }
    public string? Notes { get; set; }
    public DateOnly ReceivedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
