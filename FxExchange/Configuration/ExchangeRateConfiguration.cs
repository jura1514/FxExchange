namespace FxExchange.Configuration;

public class ExchangeRateConfiguration
{
    public const string SectionName = "ExchangeRates";

    public bool LoadFromConfig { get; init; }

    public List<CurrencyRateConfig> Currencies { get; init; } = [];
}