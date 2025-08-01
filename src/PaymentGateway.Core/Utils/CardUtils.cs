using PaymentGateway.Core.Constants;
namespace PaymentGateway.Core.Utils;

public static class CardUtils
{
    public static bool IsValidLength(string cardNumber)
    {
        return !string.IsNullOrWhiteSpace(cardNumber) &&
               cardNumber.Length >= PaymentConstants.MinCardNumberLength &&
               cardNumber.Length <= PaymentConstants.MaxCardNumberLength;
    }

    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            return "****";

        var lastFour = GetLastFourDigits(cardNumber);
        return $"**** **** **** {lastFour}";
    }

    public static string GetLastFourDigits(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            return "****";

        return cardNumber[^4..];
    }

    public static bool IsNumericOnly(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.All(char.IsDigit);
    }

    public static bool IsValidCvv(string cvv)
    {
        return !string.IsNullOrWhiteSpace(cvv) &&
               cvv.Length >= PaymentConstants.MinCvvLength &&
               cvv.Length <= PaymentConstants.MaxCvvLength &&
               IsNumericOnly(cvv);
    }

    public static bool IsValidExpiryMonth(int month)
    {
        return month >= 1 && month <= 12;
    }

    public static bool IsExpiryDateInFuture(int month, int year)
    {
        try
        {
            var expiryDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return expiryDate > DateTime.Now;
        }
        catch
        {
            return false;
        }
    }
}