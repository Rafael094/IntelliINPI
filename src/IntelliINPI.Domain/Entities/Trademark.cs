namespace IntelliINPI.Domain.Entities;

public sealed class Trademark
{
    public Guid Id { get; set; }
    public string ProcessNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? OwnerId { get; set; }
    public TrademarkOwner? Owner { get; set; }
    public Guid? StatusId { get; set; }
    public TrademarkStatus? Status { get; set; }
    public DateOnly? FilingDate { get; set; }
    public DateOnly? RegistrationDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? Presentation { get; set; }
    public string? Nature { get; set; }
    public string? LegalRepresentative { get; set; }
    public string? InpiDetailUrl { get; set; }
    public string? LogoPath { get; set; }
    public string? LogoContentType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<TrademarkNiceClass> NiceClasses { get; set; } = [];
    public ICollection<TrademarkViennaClass> ViennaClasses { get; set; } = [];
    public ICollection<TrademarkPetition> Petitions { get; set; } = [];
    public ICollection<TrademarkOwnerLink> OwnerLinks { get; set; } = [];
    public ICollection<TrademarkDispatch> Dispatches { get; set; } = [];
    public ICollection<MonitoredTrademark> Monitors { get; set; } = [];
}
