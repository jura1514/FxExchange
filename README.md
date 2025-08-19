# FX Exchange

A simple console-based currency converter built with .NET 8.0.

## Usage

```bash
dotnet run --project FxExchange
```

Enter currency pairs like `EUR/USD` and amounts to convert between supported currencies (DKK, EUR, USD, GBP, SEK, NOK, CHF, JPY).

## Configuration

Exchange rates are configurable in `appsettings.json`:

```json
{
  "ExchangeRates": {
    "Currencies": [
      {
        "Code": "EUR",
        "Name": "Euro",
        "Rate": 7.4394
      }
    ]
  }
}
```

All rates are relative to DKK as the base currency.