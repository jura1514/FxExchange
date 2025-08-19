using FxExchange.Models;

namespace FxExchange.Services;

public class ConsoleUserInterface(IExchangeRateProvider exchangeRateProvider) : IUserInterface
{
    public (Currency, Currency) GetIsoCurrencyPair()
    {
        Console.Write("Enter currency pair, e.g. EUR/DKK: ");
        var input = Console.ReadLine()?.ToUpperInvariant();

        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Currency pair cannot be empty.");

        var currencies = input.Split('/');
        if (currencies.Length != 2)
            throw new ArgumentException("Currency pair must be in the format 'CURRENCY1/CURRENCY2'.");

        var baseCurrencyCode = currencies[0];
        var quoteCurrencyCode = currencies[1];

        if (!exchangeRateProvider.TryGetCurrency(baseCurrencyCode, out var baseCurrency) ||
            !exchangeRateProvider.TryGetCurrency(quoteCurrencyCode, out var quoteCurrency))
            throw new ArgumentException(
                $"Exchange rates not available for currency pair: {input}.\nSupported currencies are:\n{string.Join(", \n", exchangeRateProvider.GetSupportedCurrencies())}.");

        return (baseCurrency, quoteCurrency);
    }

    public decimal GetAmount()
    {
        Console.Write("Enter amount to convert: ");
        var input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) || !decimal.TryParse(input, out var amount) || amount <= 0)
        {
            throw new ArgumentException("Amount must be a positive number.");
        }

        return amount;
    }

    public void DisplayResult(decimal amount, Currency baseCurrency, decimal convertedAmount, Currency quoteCurrency)
    {
        Console.WriteLine($"{amount} {baseCurrency.ToString()} = {convertedAmount:F2} {quoteCurrency.ToString()}");
    }
}