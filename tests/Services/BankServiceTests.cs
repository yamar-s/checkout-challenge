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

public class BankServiceTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsAuthorized_WhenCardEndsWithOdd()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        var logger = NullLogger<BankService>.Instance;
        var service = new BankService(httpClient, logger);

        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var response = await service.ProcessPaymentAsync(request);

        Assert.True(response.IsSuccess);
        Assert.False(string.IsNullOrEmpty(response.AuthorizationCode));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsNotAuthorized_WhenCardEndsWithEven()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        var logger = NullLogger<BankService>.Instance;
        var service = new BankService(httpClient, logger);

        var request = new BankRequest
        {
            CardNumber = "1234567890123452",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var response = await service.ProcessPaymentAsync(request);

        Assert.False(response.IsSuccess);
        Assert.True(string.IsNullOrEmpty(response.AuthorizationCode));
    }
    
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsNotAuthorized_WhenCardEndsWithZero()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        var logger = NullLogger<BankService>.Instance;
        var service = new BankService(httpClient, logger);

        var request = new BankRequest
        {
            CardNumber = "1234567890123450", 
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var response = await service.ProcessPaymentAsync(request);
        Assert.False(response.IsSuccess);
        Assert.True(string.IsNullOrEmpty(response.AuthorizationCode));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsError_WhenHttpFails()
    {
        var httpClient = HttpClientMockFactory.CreateWithException(new HttpRequestException("Network error"));
        var logger = NullLogger<BankService>.Instance;
        var service = new BankService(httpClient, logger);
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await service.ProcessPaymentAsync(request);
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsError_WhenResponseIsNotSuccess()
    {
        var httpClient = HttpClientMockFactory.CreateWithResponse(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("Server error")
        });
        var logger = NullLogger<BankService>.Instance;
        var service = new BankService(httpClient, logger);
        var request = new BankRequest
        {
            CardNumber = "1234567890123451",
            ExpiryDate = "12/2025",
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await service.ProcessPaymentAsync(request);
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }
}