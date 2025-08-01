using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Utils;

namespace PaymentGateway.Core.Mappers;

public static class PaymentMapper
{
    public static BankRequest ToBankRequest(ProcessPaymentRequest request)
    {
        return new BankRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency.ToUpperInvariant(),
            Amount = request.Amount,
            Cvv = request.Cvv
        };
    }

    public static Payment ToPaymentEntity(ProcessPaymentRequest request, BankResponse bankResponse)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Status = bankResponse.IsSuccess ? PaymentStatus.Authorized : PaymentStatus.Declined,
            AuthorizationCode = bankResponse.AuthorizationCode,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ProcessPaymentResponse ToProcessPaymentResponse(Payment payment, BankResponse bankResponse)
    {
        return new ProcessPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardLastFour = payment.CardLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount,
            IsSuccess = bankResponse.IsSuccess,
            AuthorizationCode = payment.AuthorizationCode
        };
    }

    public static Payment ToRejectedPaymentEntity(ProcessPaymentRequest request)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            CardLastFour = CardUtils.GetLastFourDigits(request.CardNumber),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency?.ToUpperInvariant() ?? string.Empty,
            Amount = request.Amount,
            Status = PaymentStatus.Rejected,
            AuthorizationCode = null,
            CreatedAt = DateTime.UtcNow
        };
    }
}