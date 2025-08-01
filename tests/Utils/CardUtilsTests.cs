using Xunit;
using PaymentGateway.Core.Utils;

public class CardUtilsTests
{
    [Fact]
    public void GetCardLastFour_ReturnsLastFourDigits()
    {
        var result = CardUtils.GetLastFourDigits("1234567890123456");
        Assert.Equal("3456", result);
    }

    [Theory]
    [InlineData("1234567890123456", true)]
    [InlineData("1234", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("123", false)]
    public void IsValidLength_Works(string cardNumber, bool expected)
    {
        Assert.Equal(expected, CardUtils.IsValidLength(cardNumber));
    }

    [Theory]
    [InlineData("1234567890123456", "**** **** **** 3456")]
    [InlineData("1234", "**** **** **** 1234")]
    [InlineData("123", "****")]
    [InlineData("", "****")]
    [InlineData(null, "****")]
    public void MaskCardNumber_Works(string cardNumber, string expected)
    {
        Assert.Equal(expected, CardUtils.MaskCardNumber(cardNumber));
    }

    [Theory]
    [InlineData("1234567890123456", "3456")]
    [InlineData("1234", "1234")]
    [InlineData("123", "****")]
    [InlineData("", "****")]
    [InlineData(null, "****")]
    public void GetLastFourDigits_Works(string cardNumber, string expected)
    {
        Assert.Equal(expected, CardUtils.GetLastFourDigits(cardNumber));
    }

    [Theory]
    [InlineData("123456", true)]
    [InlineData("0000", true)]
    [InlineData("12a4", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNumericOnly_Works(string input, bool expected)
    {
        Assert.Equal(expected, CardUtils.IsNumericOnly(input));
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("12", true)]
    [InlineData("1", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("12a", false)]
    [InlineData("12345", false)]
    public void IsValidCvv_Works(string cvv, bool expected)
    {
        Assert.Equal(expected, CardUtils.IsValidCvv(cvv));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(12, true)]
    [InlineData(0, false)]
    [InlineData(13, false)]
    public void IsValidExpiryMonth_Works(int month, bool expected)
    {
        Assert.Equal(expected, CardUtils.IsValidExpiryMonth(month));
    }

    [Fact]
    public void IsExpiryDateInFuture_Works()
    {
        var now = DateTime.Now;
        Assert.True(CardUtils.IsExpiryDateInFuture(now.Month, now.Year));
        Assert.False(CardUtils.IsExpiryDateInFuture(1, 2000));
        Assert.False(CardUtils.IsExpiryDateInFuture(13, now.Year));
    }

    [Fact]
    public void MaskCardNumber_ReturnsMask_WhenInputIsNullOrShort()
    {
        Assert.Equal("****", CardUtils.MaskCardNumber(null));
        Assert.Equal("****", CardUtils.MaskCardNumber(""));
        Assert.Equal("****", CardUtils.MaskCardNumber("12"));
    }

    [Fact]
    public void GetLastFourDigits_ReturnsMask_WhenInputIsNullOrShort()
    {
        Assert.Equal("****", CardUtils.GetLastFourDigits(null));
        Assert.Equal("****", CardUtils.GetLastFourDigits(""));
        Assert.Equal("****", CardUtils.GetLastFourDigits("12"));
    }

    [Fact]
    public void IsValidCvv_ReturnsFalse_WhenInputIsNullOrInvalid()
    {
        Assert.False(CardUtils.IsValidCvv(null));
        Assert.False(CardUtils.IsValidCvv(""));
        Assert.False(CardUtils.IsValidCvv("abc"));
        Assert.False(CardUtils.IsValidCvv("12345"));
    }

    [Fact]
    public void IsValidExpiryMonth_ReturnsFalse_WhenOutOfRange()
    {
        Assert.False(CardUtils.IsValidExpiryMonth(0));
        Assert.False(CardUtils.IsValidExpiryMonth(13));
    }

    [Fact]
    public void IsExpiryDateInFuture_ReturnsFalse_WhenInvalidDate()
    {
        Assert.False(CardUtils.IsExpiryDateInFuture(0, 2025));
        Assert.False(CardUtils.IsExpiryDateInFuture(13, 2025));
        Assert.False(CardUtils.IsExpiryDateInFuture(1, 2000));
    }
}
