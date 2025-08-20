using FxExchange.Models;
using FxExchange.Services;
using FxExchange.Utils;
using System.Globalization;

namespace FxExchange.Tests.Services;

public class ConsoleUserInterfaceTests
{
    private readonly Mock<IExchangeRateProvider> _mockExchangeRateProvider;
    private readonly Mock<IConsoleHelper> _mockConsoleHelper;
    private readonly ConsoleUserInterface _consoleUserInterface;

    public ConsoleUserInterfaceTests()
    {
        _mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        _mockConsoleHelper = new Mock<IConsoleHelper>();
        _consoleUserInterface = new ConsoleUserInterface(_mockExchangeRateProvider.Object, _mockConsoleHelper.Object);
    }

    [Theory]
    [InlineData("EUR/USD")]
    [InlineData("eur/usd")]
    public void GetIsoCurrencyPair_ValidInput_ReturnsCorrectCurrencyPair(string input)
    {
        SetupValidCurrencyPair();
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var result = _consoleUserInterface.GetIsoCurrencyPair();

        Assert.Equal("EUR", result.Item1.Code);
        Assert.Equal("USD", result.Item2.Code);
        _mockConsoleHelper.Verify(x => x.Write("Enter currency pair, e.g. EUR/DKK: "), Times.Once);
        _mockConsoleHelper.Verify(x => x.ReadLine(), Times.Once);
    }

    [Fact]
    public void GetIsoCurrencyPair_EmptyInput_ThrowsArgumentException()
    {
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns("");

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetIsoCurrencyPair());
        Assert.Equal("Currency pair cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("EURUSD")]
    [InlineData("EUR/USD/GBP")]
    public void GetIsoCurrencyPair_InvalidFormat_ThrowsArgumentException(string input)
    {
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetIsoCurrencyPair());
        Assert.Equal("Currency pair must be in the format 'CURRENCY1/CURRENCY2'.", exception.Message);
    }

    [Fact]
    public void GetIsoCurrencyPair_UnsupportedBaseCurrency_ThrowsArgumentException()
    {
        const string input = "XXX/USD";
        SetupUnsupportedCurrency("XXX", "USD");
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetIsoCurrencyPair());
        Assert.Contains("Exchange rates not available for currency pair: XXX/USD", exception.Message);
        Assert.Contains("Supported currencies are:", exception.Message);
    }

    [Fact]
    public void GetIsoCurrencyPair_UnsupportedQuoteCurrency_ThrowsArgumentException()
    {
        const string input = "EUR/XXX";
        SetupUnsupportedCurrency("XXX", "EUR");
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetIsoCurrencyPair());
        Assert.Contains("Exchange rates not available for currency pair: EUR/XXX", exception.Message);
        Assert.Contains("Supported currencies are:", exception.Message);
    }

    [Fact]
    public void GetAmount_ValidDecimalInput_ReturnsCorrectAmount()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator;
        var input = $"100{decimalSeparator}50";
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var result = _consoleUserInterface.GetAmount();

        Assert.Equal(100.50m, result);
        _mockConsoleHelper.Verify(x => x.Write("Enter amount to convert: "), Times.Once);
        _mockConsoleHelper.Verify(x => x.ReadLine(), Times.Once);
    }

    [Fact]
    public void GetAmount_ValidIntegerInput_ReturnsCorrectAmount()
    {
        const string input = "100";
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var result = _consoleUserInterface.GetAmount();

        Assert.Equal(100m, result);
    }

    [Fact]
    public void GetAmount_EmptyInput_ThrowsArgumentException()
    {
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns("");

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetAmount());
        Assert.Equal("Amount must be a positive number.", exception.Message);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("not_a_number")]
    [InlineData("0")]
    [InlineData("-10")]
    public void GetAmount_InvalidInput_ThrowsArgumentException(string input)
    {
        _mockConsoleHelper.Setup(x => x.ReadLine()).Returns(input);

        var exception = Assert.Throws<ArgumentException>(() => _consoleUserInterface.GetAmount());
        Assert.Equal("Amount must be a positive number.", exception.Message);
    }

    [Fact]
    public void DisplayResult_ValidInputs_DisplaysCorrectFormat()
    {
        const decimal amount = 100m;
        var baseCurrency = new Currency("EUR", "Euro");
        const decimal convertedAmount = 108.50m;
        var quoteCurrency = new Currency("USD", "US Dollar");

        _consoleUserInterface.DisplayResult(amount, baseCurrency, convertedAmount, quoteCurrency);

        _mockConsoleHelper.Verify(x => x.WriteLine(It.Is<string>(s =>
            s.Contains("100") &&
            s.Contains("EUR (Euro)") &&
            s.Contains("108") &&
            s.Contains("50") &&
            s.Contains("USD (US Dollar)"))), Times.Once);
    }

    [Fact]
    public void DisplayResult_DecimalAmounts_DisplaysWithCorrectPrecision()
    {
        const decimal amount = 123.456m;
        var baseCurrency = new Currency("GBP", "British Pound");
        const decimal convertedAmount = 150.789123m;
        var quoteCurrency = new Currency("USD", "US Dollar");

        _consoleUserInterface.DisplayResult(amount, baseCurrency, convertedAmount, quoteCurrency);

        _mockConsoleHelper.Verify(x => x.WriteLine(It.Is<string>(s =>
            s.Contains("123") &&
            s.Contains("456") &&
            s.Contains("GBP (British Pound)") &&
            s.Contains("150") &&
            s.Contains("79") &&
            s.Contains("USD (US Dollar)"))), Times.Once);
    }

    private void SetupValidCurrencyPair(string baseCurrencyCode = "EUR", string quoteCurrencyCode = "USD")
    {
        var baseCurrency = new Currency(baseCurrencyCode, GetCurrencyName(baseCurrencyCode));
        var quoteCurrency = new Currency(quoteCurrencyCode, GetCurrencyName(quoteCurrencyCode));

        _mockExchangeRateProvider.Setup(x => x.TryGetCurrency(baseCurrencyCode, out baseCurrency))
            .Returns(true);
        _mockExchangeRateProvider.Setup(x => x.TryGetCurrency(quoteCurrencyCode, out quoteCurrency))
            .Returns(true);
    }

    private void SetupUnsupportedCurrency(string unsupportedCurrency, string supportedCurrency)
    {
        var supported = new Currency(supportedCurrency, GetCurrencyName(supportedCurrency));
        var supportedCurrencies = new[] { new Currency("EUR", "Euro"), supported };

        Currency outCurrency;
        _mockExchangeRateProvider.Setup(x => x.TryGetCurrency(unsupportedCurrency, out outCurrency))
            .Returns(false);
        _mockExchangeRateProvider.Setup(x => x.TryGetCurrency(supportedCurrency, out supported))
            .Returns(true);
        _mockExchangeRateProvider.Setup(x => x.GetSupportedCurrencies())
            .Returns(supportedCurrencies);
    }

    private static string GetCurrencyName(string currencyCode) => currencyCode switch
    {
        "EUR" => "Euro",
        "USD" => "US Dollar",
        "GBP" => "British Pound",
        _ => $"{currencyCode} Currency"
    };
}