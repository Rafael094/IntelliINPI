namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiRpiOptions
{
    public string BaseUrl { get; set; } = "https://revistas.inpi.gov.br/rpi/";
    public string RawDirectory { get; set; } = "data/inpi/rpi/raw";
}
