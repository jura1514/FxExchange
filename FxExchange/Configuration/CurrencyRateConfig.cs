namespace FxExchange.Configuration;

public class CurrencyRateConfig
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public decimal Rate { get; init; }
}