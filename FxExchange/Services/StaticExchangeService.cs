using FxExchange.Models;

namespace FxExchange.Services;

public class StaticExchangeService(IExchangeRateProvider exchangeRateProvider, IUserInterface userInterface)
    : IExchangeService
{
    public void ProcessCurrencyExchange()
    {
        var (baseCurrency, quoteCurrency) = userInterface.GetIsoCurrencyPair();
        var amount = userInterface.GetAmount();
        var convertedAmount = ConvertCurrency(baseCurrency, quoteCurrency, amount);
        userInterface.DisplayResult(amount, baseCurrency, convertedAmount, quoteCurrency);
    }

    private decimal ConvertCurrency(Currency baseCurrency, Currency quoteCurrency, decimal amount)
    {
        var baseRateInDkk = exchangeRateProvider.GetRate(baseCurrency);
        var quoteRateInDkk = exchangeRateProvider.GetRate(quoteCurrency);

        var amountInDkk = amount / baseRateInDkk;
        var amountInQuoteCurrency = amountInDkk * quoteRateInDkk;

        return amountInQuoteCurrency;
    }
}