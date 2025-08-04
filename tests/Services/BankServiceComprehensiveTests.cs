using Xunit;
using PaymentGateway.Core.Services;
using PaymentGateway.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Text;
using System.Text.Json;

namespace PaymentGateway.Tests.Services;

public class BankServiceComprehensiveTests
{
    [Theory]
    [InlineData("1234567890123451")] // Odd = Success
    [InlineData("1234567890123453")]
    [InlineData("1234567890123455")]
    public async Task ProcessPaymentAsync_OddCardNumbers_ReturnsSuccess(string cardNumber)
    {
        // Arrange
        var httpClient = CreateSuccessfulHttpClient();
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = cardNumber,
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.AuthorizationCode);
        Assert.True(response.AuthorizationCode.Length > 0);
    }

    [Theory]
    [InlineData("1234567890123452")] // Even = Failure
    [InlineData("1234567890123454")]
    [InlineData("1234567890123456")]
    public async Task ProcessPaymentAsync_EvenCardNumbers_ReturnsFailure(string cardNumber)
    {
        // Arrange
        var httpClient = CreateFailureHttpClient();
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = cardNumber,
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_NetworkError_ReturnsFailure()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("Network error", response.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_Timeout_ReturnsFailure()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("timeout", response.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessPaymentAsync_BadResponse_ReturnsFailure()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad request")
            });

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Invalid JSON")
            });

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task RefundPaymentAsync_WithValidAuthCode_CompletesSuccessfully()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);

        // Act & Assert - Should not throw
        await service.RefundPaymentAsync(null, "AUTH123");
        
        // Verify DELETE request was made
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Delete && 
                req.RequestUri.ToString().Contains("AUTH123")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RefundPaymentAsync_NetworkError_ThrowsException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
        
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            service.RefundPaymentAsync(null, "AUTH123"));
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public async Task ProcessPaymentAsync_DifferentCurrencies_HandledCorrectly(string currency)
    {
        // Arrange
        var httpClient = CreateSuccessfulHttpClient();
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = currency,
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    public async Task ProcessPaymentAsync_DifferentAmounts_HandledCorrectly(long amount)
    {
        // Arrange
        var httpClient = CreateSuccessfulHttpClient();
        var service = new BankService(httpClient, NullLogger<BankService>.Instance);
        
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = amount,
            Cvv = "123"
        };

        // Act
        var response = await service.ProcessPaymentAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
    }

    private static HttpClient CreateSuccessfulHttpClient()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var successResponse = new BankResponse 
        { 
            IsSuccess = true, 
            AuthorizationCode = "AUTH" + Guid.NewGuid().ToString("N")[..8] 
        };
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(successResponse))
            });

        return new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
    }

    private static HttpClient CreateFailureHttpClient()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var failureResponse = new BankResponse 
        { 
            IsSuccess = false, 
            ErrorMessage = "Insufficient funds" 
        };
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(failureResponse))
            });

        return new HttpClient(mockHandler.Object) 
        { 
            BaseAddress = new Uri("http://localhost") 
        };
    }
}
