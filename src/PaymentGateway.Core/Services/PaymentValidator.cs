using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Core.Services;

public class PaymentValidator : IPaymentValidator
{
    private static readonly string[] SupportedCurrencies = { "USD", "EUR", "GBP" };

    public Task<ValidationResult> ValidateAsync(ProcessPaymentRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.CardNumber))
            result.AddError("CardNumber is required");
        else if (!CardUtils.IsNumericOnly(request.CardNumber))
            result.AddError("CardNumber must contain only digits");
        else if (!CardUtils.IsValidLength(request.CardNumber))
            result.AddError("Card number must be between 14 and 19 digits");

        if (string.IsNullOrWhiteSpace(request.Cvv))
            result.AddError("CVV is required");
        else if (!CardUtils.IsValidCvv(request.Cvv))
            result.AddError("CVV must be 3 or 4 digits");

        if (!CardUtils.IsValidExpiryMonth(request.ExpiryMonth))
            result.AddError("Month must be between 1-12");
        if (!CardUtils.IsExpiryDateInFuture(request.ExpiryMonth, request.ExpiryYear))
            result.AddError("Expiry date must be in future");

        if (request.Amount <= 0)
            result.AddError("Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(request.Currency))
            result.AddError("Currency is required");
        else if (!SupportedCurrencies.Contains(request.Currency.ToUpperInvariant()))
            result.AddError($"Currency must be one of: {string.Join(", ", SupportedCurrencies)}");

        return Task.FromResult(result);
    }
}