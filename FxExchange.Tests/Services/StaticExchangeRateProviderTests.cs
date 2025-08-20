using FxExchange.Configuration;
using FxExchange.Models;
using FxExchange.Services;
using Microsoft.Extensions.Options;

namespace FxExchange.Tests.Services;

public class StaticExchangeRateProviderTests
{
    public static TheoryData<string, string, decimal> StaticCurrencyRateData =>
        new()
        {
            { "DKK", "Danish kroner", 1.0m },
            { "EUR", "Euro", 7.4394m },
            { "USD", "Amerikanske dollar", 6.6311m },
            { "GBP", "Britiske pund", 8.5285m },
            { "SEK", "Svenske kroner", 0.7610m },
            { "NOK", "Norske kroner", 0.7840m },
            { "CHF", "Schweiziske franc", 6.8358m },
            { "JPY", "Japanske yen", 0.059740m }
        };

    public static TheoryData<string, bool, string?> CurrencyLookupData =>
        new()
        {
            { "EUR", true, "EUR" },
            { "eur", true, "EUR" },
            { "USD", true, "USD" },
            { "usd", true, "USD" },
            { "GBP", true, "GBP" },
            { "gbp", true, "GBP" },
            { "XYZ", false, null },
            { "abc", false, null },
            { "", false, null },
            { "12", false, null }
        };

    public static TheoryData<ExchangeRateConfiguration, int, string[]> ConfigurationTestData =>
        new()
        {
            {
                new ExchangeRateConfiguration { LoadFromConfig = false, Currencies = [] },
                8,
                ["DKK", "EUR", "USD", "GBP", "SEK", "NOK", "CHF", "JPY"]
            },
            {
                new ExchangeRateConfiguration
                {
                    LoadFromConfig = true,
                    Currencies =
                    [
                        new CurrencyRateConfig { Code = "TS1", Name = "Test Currency 1", Rate = 10.5m },
                        new CurrencyRateConfig { Code = "TS2", Name = "Test Currency 2", Rate = 20.75m }
                    ]
                },
                2,
                ["TS1", "TS2"]
            },
            {
                new ExchangeRateConfiguration
                {
                    LoadFromConfig = true,
                    Currencies =
                    [
                        new CurrencyRateConfig { Code = "AAA", Name = "Currency A", Rate = 1.0m },
                        new CurrencyRateConfig { Code = "BBB", Name = "Currency B", Rate = 2.0m },
                        new CurrencyRateConfig { Code = "CCC", Name = "Currency C", Rate = 3.0m }
                    ]
                },
                3,
                ["AAA", "BBB", "CCC"]
            }
        };

    [Fact]
    public void GetRate_ValidCurrency_ReturnsCorrectRate()
    {
        var provider = CreateProviderWithStaticRates();
        var eurCurrency = new Currency("EUR", "Euro");

        var rate = provider.GetRate(eurCurrency);

        Assert.Equal(7.4394m, rate);
    }

    [Fact]
    public void GetRate_UnsupportedCurrency_ThrowsArgumentException()
    {
        var provider = CreateProviderWithStaticRates();
        var unsupportedCurrency = new Currency("XYZ", "Unknown Currency");

        var exception = Assert.Throws<ArgumentException>(() => provider.GetRate(unsupportedCurrency));
        Assert.Contains("Currency 'XYZ (Unknown Currency)' is not supported.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(StaticCurrencyRateData))]
    public void GetRate_StaticCurrencies_ReturnsExpectedRates(string code, string name, decimal expectedRate)
    {
        var provider = CreateProviderWithStaticRates();
        var currency = new Currency(code, name);

        var rate = provider.GetRate(currency);

        Assert.Equal(expectedRate, rate);
    }

    [Fact]
    public void GetSupportedCurrencies_StaticRates_ReturnsAllStaticCurrencies()
    {
        var provider = CreateProviderWithStaticRates();

        var currencies = provider.GetSupportedCurrencies().ToList();

        Assert.Equal(8, currencies.Count);
        Assert.Contains(currencies, c => c.Code == "DKK");
        Assert.Contains(currencies, c => c.Code == "EUR");
        Assert.Contains(currencies, c => c.Code == "USD");
        Assert.Contains(currencies, c => c.Code == "GBP");
        Assert.Contains(currencies, c => c.Code == "SEK");
        Assert.Contains(currencies, c => c.Code == "NOK");
        Assert.Contains(currencies, c => c.Code == "CHF");
        Assert.Contains(currencies, c => c.Code == "JPY");
    }

    [Theory]
    [MemberData(nameof(CurrencyLookupData))]
    public void TryGetCurrency_VariousInputs_ReturnsExpectedResults(string code, bool expectedResult,
        string? expectedCode)
    {
        var provider = CreateProviderWithStaticRates();

        var result = provider.TryGetCurrency(code, out var currency);

        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(expectedCode, currency.Code);
        }
        else
        {
            Assert.Equal(default, currency);
        }
    }

    [Theory]
    [MemberData(nameof(ConfigurationTestData))]
    public void Constructor_DifferentConfigurations_LoadsCorrectCurrencies(
        ExchangeRateConfiguration config,
        int expectedCount,
        string[] expectedCodes)
    {
        var options = CreateOptions(config);

        var provider = new StaticExchangeRateProvider(options);

        var currencies = provider.GetSupportedCurrencies().ToList();
        Assert.Equal(expectedCount, currencies.Count);

        foreach (var expectedCode in expectedCodes)
        {
            Assert.Contains(currencies, c => c.Code == expectedCode);
        }
    }

    [Fact]
    public void GetRate_ConfigurationRates_ReturnsConfiguredRate()
    {
        var config = CreateConfigurationWithCustomRates();
        var options = CreateOptions(config);
        var provider = new StaticExchangeRateProvider(options);
        var testCurrency = new Currency("TS1", "Test Currency 1");

        var rate = provider.GetRate(testCurrency);

        Assert.Equal(10.5m, rate);
    }

    [Fact]
    public void TryGetCurrency_ConfigurationRates_FindsConfiguredCurrency()
    {
        var config = CreateConfigurationWithCustomRates();
        var options = CreateOptions(config);
        var provider = new StaticExchangeRateProvider(options);

        var result = provider.TryGetCurrency("TS1", out var currency);

        Assert.True(result);
        Assert.Equal("TS1", currency.Code);
        Assert.Equal("Test Currency 1", currency.Name);
    }

    [Fact]
    public void TryGetCurrency_NullInput_ReturnsFalse()
    {
        var provider = CreateProviderWithStaticRates();

        var result = provider.TryGetCurrency(null!, out var currency);

        Assert.False(result);
        Assert.Equal(default, currency);
    }

    private static StaticExchangeRateProvider CreateProviderWithStaticRates()
    {
        var config = new ExchangeRateConfiguration
        {
            LoadFromConfig = false,
            Currencies = []
        };
        var options = CreateOptions(config);
        return new StaticExchangeRateProvider(options);
    }

    private static ExchangeRateConfiguration CreateConfigurationWithCustomRates()
    {
        return new ExchangeRateConfiguration
        {
            LoadFromConfig = true,
            Currencies =
            [
                new CurrencyRateConfig { Code = "TS1", Name = "Test Currency 1", Rate = 10.5m },
                new CurrencyRateConfig { Code = "TS2", Name = "Test Currency 2", Rate = 20.75m }
            ]
        };
    }

    private static IOptions<ExchangeRateConfiguration> CreateOptions(ExchangeRateConfiguration config)
    {
        var mock = new Mock<IOptions<ExchangeRateConfiguration>>();
        mock.Setup(x => x.Value).Returns(config);
        return mock.Object;
    }
}