namespace IntelliINPI.Domain.Entities;

public sealed class NitUserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
