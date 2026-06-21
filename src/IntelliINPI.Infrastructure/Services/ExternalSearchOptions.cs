namespace IntelliINPI.Infrastructure.Services;

public sealed class ExternalSearchOptions
{
    public string Provider { get; set; } = "None";
    public string? ApiKey { get; set; }
    public int MaxResultsPerQuery { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 20;
}
