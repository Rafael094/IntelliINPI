using IntelliINPI.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiRpiHistoryOptions
{
    public Dictionary<int, int> YearStartRpiMap { get; set; } = [];
}

public sealed class InpiRpiHistorySettings(
    IOptions<InpiRpiHistoryOptions> historyOptions,
    IOptions<InpiRpiOptions> rpiOptions) : IInpiRpiHistorySettings
{
    private readonly InpiRpiHistoryOptions _historyOptions = historyOptions.Value;
    private readonly InpiRpiOptions _rpiOptions = rpiOptions.Value;

    public string RawDirectory => _rpiOptions.RawDirectory;

    public int? GetStartRpiForYear(int year)
    {
        return _historyOptions.YearStartRpiMap.TryGetValue(year, out var rpiNumber) ? rpiNumber : null;
    }
}
