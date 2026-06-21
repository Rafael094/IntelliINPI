namespace IntelliINPI.Domain.Entities;

public sealed class InventionResearcher
{
    public Guid InventionId { get; set; }
    public Invention Invention { get; set; } = null!;
    public Guid ResearcherId { get; set; }
    public Researcher Researcher { get; set; } = null!;
}
