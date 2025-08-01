using Xunit;
using PaymentGateway.Core.Services;
using PaymentGateway.Core.Models;
using System.Threading.Tasks;

public class PaymentValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenAmountIsZero()
    {
        var validator = new PaymentValidator();
        var request = new ProcessPaymentRequest { Amount = 0 };
        var result = await validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenCardNumberIsInvalid()
    {
        var validator = new PaymentValidator();
        var request = new ProcessPaymentRequest { CardNumber = "123" };
        var result = await validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenExpiryDateIsInvalid()
    {
        var validator = new PaymentValidator();
        var request = new ProcessPaymentRequest { ExpiryMonth = 13, ExpiryYear = 2023 };
        var result = await validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenCvvIsInvalid()
    {
        var validator = new PaymentValidator();
        var request = new ProcessPaymentRequest { Cvv = "12" };
        var result = await validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsValid_WhenRequestIsValid()
    {
        var validator = new PaymentValidator();
        var request = new ProcessPaymentRequest
        {
            Amount = 100,
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Cvv = "123",
            Currency = "USD"
        };
        var result = await validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }
}