namespace PaymentGateway.Core.Enums;

public enum Currency
{
    USD,
    EUR,
    GBP
}

public static class CurrencyExtensions
{
    public static bool IsSupported(this string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) &&
               Enum.TryParse<Currency>(currency.ToUpperInvariant(), out _);
    }

    public static string GetSupportedDisplay()
    {
        return string.Join(", ", Enum.GetNames<Currency>());
    }

}