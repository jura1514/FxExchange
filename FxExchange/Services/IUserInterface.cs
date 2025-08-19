using FxExchange.Models;

namespace FxExchange.Services;

public interface IUserInterface
{
    (Currency, Currency) GetIsoCurrencyPair();
    decimal GetAmount();
    void DisplayResult(decimal amount, Currency baseCurrency, decimal convertedAmount, Currency quoteCurrency);
}