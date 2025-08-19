using FxExchange.Models;

namespace FxExchange.Services;

public interface IExchangeRateProvider
{
    decimal GetRate(Currency currency);
    IEnumerable<Currency> GetSupportedCurrencies();
    bool TryGetCurrency(string code, out Currency currency);
}