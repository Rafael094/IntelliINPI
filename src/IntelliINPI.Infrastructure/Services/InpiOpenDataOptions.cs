namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiOpenDataOptions
{
    public string BaseUrl { get; set; } = "https://dadosabertos.inpi.gov.br/index/marcas/";
    public string RawDirectory { get; set; } = "data/inpi/raw";
    public string[] TrademarkFileNames { get; set; } =
    [
        "marcas_dados_bibliograficos",
        "marcas_depositantes",
        "marcas_classificacoes_nice",
        "marcas_despachos"
    ];
}
