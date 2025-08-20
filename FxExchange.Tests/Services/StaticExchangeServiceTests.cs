using FxExchange.Models;
using FxExchange.Services;

namespace FxExchange.Tests.Services;

public class StaticExchangeServiceTests
{
    public static TheoryData<decimal, Currency, Currency, decimal, decimal, decimal> CurrencyConversionData =>
        new()
        {
            { 100m, new Currency("EUR", "Euro"), new Currency("USD", "US Dollar"), 7.4394m, 6.6311m, 89.13m },
            { 50m, new Currency("USD", "US Dollar"), new Currency("EUR", "Euro"), 6.6311m, 7.4394m, 56.09m },
            {
                1000m, new Currency("DKK", "Danish Kroner"), new Currency("GBP", "British Pound"), 1.0m, 8.5285m,
                8528.50m
            },
            { 100m, new Currency("DKK", "Danish Kroner"), new Currency("EUR", "Euro"), 1.0m, 7.4394m, 743.94m },
            { 100m, new Currency("EUR", "Euro"), new Currency("DKK", "Danish Kroner"), 7.4394m, 1.0m, 13.44m },
            {
                25m, new Currency("GBP", "British Pound"), new Currency("SEK", "Swedish Krona"), 8.5285m, 0.7610m, 2.23m
            },
            {
                200m, new Currency("CHF", "Swiss Franc"), new Currency("NOK", "Norwegian Krone"), 6.8358m, 0.7840m,
                22.94m
            },
            { 0m, new Currency("EUR", "Euro"), new Currency("USD", "US Dollar"), 7.4394m, 6.6311m, 0m },
            { 1000000m, new Currency("EUR", "Euro"), new Currency("USD", "US Dollar"), 7.4394m, 6.6311m, 891348.77m },
            { 100m, new Currency("EUR", "Euro"), new Currency("EUR", "Euro"), 7.4394m, 7.4394m, 100m }
        };

    [Fact]
    public void ProcessCurrencyExchange_ValidInputs_CallsAllInterfaceMethods()
    {
        var mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        var mockUserInterface = new Mock<IUserInterface>();
        var service = new StaticExchangeService(mockExchangeRateProvider.Object, mockUserInterface.Object);

        var baseCurrency = new Currency("EUR", "Euro");
        var quoteCurrency = new Currency("USD", "US Dollar");
        const decimal amount = 100m;

        SetupMocks(mockUserInterface, mockExchangeRateProvider, baseCurrency, quoteCurrency, amount, 7.4394m, 6.6311m);

        service.ProcessCurrencyExchange();

        VerifyAllMethodsCalled(mockUserInterface, mockExchangeRateProvider, baseCurrency, quoteCurrency);
    }

    [Theory]
    [MemberData(nameof(CurrencyConversionData))]
    public void ProcessCurrencyExchange_VariousScenarios_CalculatesCorrectConversion(
        decimal inputAmount,
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal baseRate,
        decimal quoteRate,
        decimal expectedResult)
    {
        var mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        var mockUserInterface = new Mock<IUserInterface>();
        var service = new StaticExchangeService(mockExchangeRateProvider.Object, mockUserInterface.Object);

        SetupMocks(mockUserInterface, mockExchangeRateProvider, baseCurrency, quoteCurrency, inputAmount, baseRate,
            quoteRate);

        service.ProcessCurrencyExchange();

        mockUserInterface.Verify(x => x.DisplayResult(
            inputAmount,
            baseCurrency,
            It.Is<decimal>(result => Math.Abs(result - expectedResult) < 0.01m),
            quoteCurrency), Times.Once);
    }


    [Fact]
    public void ProcessCurrencyExchange_ExchangeRateProviderThrows_PropagatesException()
    {
        var mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        var mockUserInterface = new Mock<IUserInterface>();
        var service = new StaticExchangeService(mockExchangeRateProvider.Object, mockUserInterface.Object);

        var baseCurrency = new Currency("EUR", "Euro");
        var quoteCurrency = new Currency("USD", "US Dollar");

        mockUserInterface.Setup(x => x.GetIsoCurrencyPair()).Returns((baseCurrency, quoteCurrency));
        mockUserInterface.Setup(x => x.GetAmount()).Returns(100m);
        mockExchangeRateProvider.Setup(x => x.GetRate(It.IsAny<Currency>()))
            .Throws(new ArgumentException("Currency not supported"));

        Assert.Throws<ArgumentException>(() => service.ProcessCurrencyExchange());
    }

    [Fact]
    public void ProcessCurrencyExchange_UserInterfaceThrows_PropagatesException()
    {
        var mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        var mockUserInterface = new Mock<IUserInterface>();
        var service = new StaticExchangeService(mockExchangeRateProvider.Object, mockUserInterface.Object);

        mockUserInterface.Setup(x => x.GetIsoCurrencyPair())
            .Throws(new ArgumentException("Invalid currency pair"));

        Assert.Throws<ArgumentException>(() => service.ProcessCurrencyExchange());
    }

    private static void SetupMocks(
        Mock<IUserInterface> mockUserInterface,
        Mock<IExchangeRateProvider> mockExchangeRateProvider,
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal amount,
        decimal baseRate,
        decimal quoteRate)
    {
        mockUserInterface.Setup(x => x.GetIsoCurrencyPair()).Returns((baseCurrency, quoteCurrency));
        mockUserInterface.Setup(x => x.GetAmount()).Returns(amount);
        mockExchangeRateProvider.Setup(x => x.GetRate(baseCurrency)).Returns(baseRate);
        mockExchangeRateProvider.Setup(x => x.GetRate(quoteCurrency)).Returns(quoteRate);
    }

    private static void VerifyAllMethodsCalled(
        Mock<IUserInterface> mockUserInterface,
        Mock<IExchangeRateProvider> mockExchangeRateProvider,
        Currency baseCurrency,
        Currency quoteCurrency)
    {
        mockUserInterface.Verify(x => x.GetIsoCurrencyPair(), Times.Once);
        mockUserInterface.Verify(x => x.GetAmount(), Times.Once);
        mockExchangeRateProvider.Verify(x => x.GetRate(baseCurrency), Times.Once);
        mockExchangeRateProvider.Verify(x => x.GetRate(quoteCurrency), Times.Once);
        mockUserInterface.Verify(
            x => x.DisplayResult(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<decimal>(), It.IsAny<Currency>()),
            Times.Once);
    }
}