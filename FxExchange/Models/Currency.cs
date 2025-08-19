namespace FxExchange.Models;

public readonly struct Currency : IEquatable<Currency>
{
    public string Code { get; }
    public string Name { get; }

    public Currency(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(code));

        Code = code.ToUpperInvariant();
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public bool Equals(Currency other)
    {
        return Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        return obj is Currency other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    public static bool operator ==(Currency left, Currency right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Currency left, Currency right)
    {
        return !left.Equals(right);
    }

    public static implicit operator string(Currency currency)
    {
        return currency.Code;
    }

    public override string ToString()
    {
        return $"{Code} ({Name})";
    }
}