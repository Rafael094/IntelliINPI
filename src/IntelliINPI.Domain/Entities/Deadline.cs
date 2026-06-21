namespace IntelliINPI.Domain.Entities;

public sealed class Deadline
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? TrademarkId { get; set; }
    public Trademark? Trademark { get; set; }
    public Guid? InventionId { get; set; }
    public Invention? Invention { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
