namespace IntelliINPI.Domain.Entities;

public sealed class PatentDispatch
{
    public Guid Id { get; set; }
    public Guid PatentId { get; set; }
    public Patent Patent { get; set; } = null!;
    public int? RpiNumber { get; set; }
    public string DispatchCode { get; set; } = string.Empty;
    public string DispatchDescription { get; set; } = string.Empty;
    public DateOnly? DispatchDate { get; set; }
    public string? Complement { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
