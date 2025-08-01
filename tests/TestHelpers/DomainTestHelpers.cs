using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Models;
using System;
using System.Collections.Generic;

namespace PaymentGateway.Tests.TestHelpers
{
    public static class CardUtilsTestHelper
    {
        public static IEnumerable<string> ValidCardNumbers()
        {
            yield return "1234567890123456";
            yield return "4111111111111111";
        }
        public static IEnumerable<string> InvalidCardNumbers()
        {
            yield return "";
            yield return null;
            yield return "123";
        }
    }

    public static class BankRequestTestHelper
    {
        public static BankRequest CreateValidBankRequest()
        {
            return new BankRequest
            {
                CardNumber = "12345678901231",
                Amount = 100,
                Currency = "USD",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            };
        }
    }
}
