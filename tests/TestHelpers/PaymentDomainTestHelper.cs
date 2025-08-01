using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Models;
using System;

namespace PaymentGateway.Tests.TestHelpers
{
    public static class PaymentDomainTestHelper
    {
        public static Payment CreateValidPayment()
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Authorized,
                CardLastFour = "1234",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Currency = "USD",
                Amount = 100,
                CreatedAt = DateTime.UtcNow,
                AuthorizationCode = "AUTH1234"
            };
        }

        public static Payment CreateInvalidPayment()
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Rejected,
                CardLastFour = "",
                ExpiryMonth = 0,
                ExpiryYear = 1999,
                Currency = "",
                Amount = 0,
                CreatedAt = DateTime.UtcNow,
                AuthorizationCode = null
            };
        }

        public static ProcessPaymentRequest CreateValidProcessPaymentRequest()
        {
            return new ProcessPaymentRequest
            {
                CardNumber = "12345678901231",
                Amount = 100,
                Currency = "USD",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            };
        }

        public static ProcessPaymentRequest CreateInvalidProcessPaymentRequest()
        {
            return new ProcessPaymentRequest
            {
                CardNumber = "",
                Amount = 0,
                Currency = "",
                ExpiryMonth = 0,
                ExpiryYear = 1999,
                Cvv = ""
            };
        }
    }
}
