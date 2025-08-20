using FxExchange.Configuration;
using FxExchange.Models;
using Microsoft.Extensions.Options;

namespace FxExchange.Services;

public class StaticExchangeRateProvider(IOptions<ExchangeRateConfiguration> options) : IExchangeRateProvider
{
    private static readonly Dictionary<Currency, decimal> StaticRatesInDkk = new()
    {
        { new Currency("DKK", "Danish kroner"), 1.0m },
        { new Currency("EUR", "Euro"), 7.4394m },
        { new Currency("USD", "Amerikanske dollar"), 6.6311m },
        { new Currency("GBP", "Britiske pund"), 8.5285m },
        { new Currency("SEK", "Svenske kroner"), 0.7610m },
        { new Currency("NOK", "Norske kroner"), 0.7840m },
        { new Currency("CHF", "Schweiziske franc"), 6.8358m },
        { new Currency("JPY", "Japanske yen"), 0.059740m }
    };

    private readonly Dictionary<Currency, decimal> _ratesInDkk = options.Value.LoadFromConfig
        ? InitializeRatesFromConfiguration(options.Value)
        : StaticRatesInDkk;

    public decimal GetRate(Currency currency)
    {
        return !_ratesInDkk.TryGetValue(currency, out var rate)
            ? throw new ArgumentException($"Currency '{currency}' is not supported.")
            : rate;
    }

    public IEnumerable<Currency> GetSupportedCurrencies()
    {
        return _ratesInDkk.Keys;
    }

    public bool TryGetCurrency(string code, out Currency currency)
    {
        currency = _ratesInDkk.Keys.FirstOrDefault(c =>
            c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase));
        return !currency.Equals(default);
    }

    private static Dictionary<Currency, decimal> InitializeRatesFromConfiguration(ExchangeRateConfiguration config)
    {
        var rates = new Dictionary<Currency, decimal>();

        foreach (var currencyConfig in config.Currencies)
        {
            var currency = new Currency(currencyConfig.Code, currencyConfig.Name);
            rates[currency] = currencyConfig.Rate;
        }

        return rates;
    }
}