using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentGateway.Tests.TestHelpers;

public class PaymentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<PaymentGateway.Api.Program>>
{
    private readonly HttpClient _client;

    public PaymentsControllerIntegrationTests(WebApplicationFactory<PaymentGateway.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostPayment_ReturnsSuccess()
    {
        var response = await PaymentApiTestHelper.PostPaymentAsync(_client, PaymentApiTestHelper.ValidPaymentPayload());
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("issuccess", responseBody.ToLower());
    }

    [Theory]
    [MemberData(nameof(PaymentApiTestHelper.InvalidPaymentPayloads), MemberType = typeof(PaymentApiTestHelper))]
    public async Task PostPayment_ReturnsBadRequest_ForInvalidPayload(object payload)
    {
        var response = await PaymentApiTestHelper.PostPaymentAsync(_client, payload);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        var randomId = System.Guid.NewGuid();
        var response = await PaymentApiTestHelper.GetPaymentAsync(_client, randomId);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostPayment_And_GetPayment_ReturnsPayment()
    {
        var id = await PaymentApiTestHelper.PostPaymentAndGetIdAsync(_client, PaymentApiTestHelper.ValidPaymentPayload());
        Assert.NotEqual(System.Guid.Empty, id);

        var getResponse = await PaymentApiTestHelper.GetPaymentAsync(_client, id);
        getResponse.EnsureSuccessStatusCode();
        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("cardLastFour", getBody.ToLower());
    }

    [Fact]
    public async Task ProcessPayment_ReturnsSuccess_WhenCardAuthorized()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "1234567890123451", 
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await _client.PostAsJsonAsync("/api/payments", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.True((bool)result.isSuccess);
        Assert.Equal("AUTHORIZED", result.status.ToString().ToUpper());
    }

    [Fact]
    public async Task ProcessPayment_ReturnsDeclined_WhenCardNotAuthorized()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "1234567890123452", 
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await _client.PostAsJsonAsync("/api/payments", request);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.False((bool)result.isSuccess);
        Assert.Equal("DECLINED", result.status.ToString().ToUpper());
    }

    [Fact]
    public async Task ProcessPayment_ReturnsError_WhenBankUnavailable()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "1234567890123450", 
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await _client.PostAsJsonAsync("/api/payments", request);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.False((bool)result.isSuccess);
        Assert.Equal("DECLINED", result.status.ToString().ToUpper());
        Assert.NotNull(result.errorMessages); // Now checks for list
        Assert.True(result.errorMessages.Count > 0); // At least one error
    }

    [Fact]
    public async Task ProcessPayment_ReturnsBadRequest_WhenInvalidRequest()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "", 
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
        var response = await _client.PostAsJsonAsync("/api/payments", request);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.False((bool)result.isSuccess);
        Assert.Equal("REJECTED", result.status.ToString().ToUpper());
        Assert.NotNull(result.errorMessages); // Now checks for list
        Assert.True(result.errorMessages.Count > 0); // At least one error
    }
}
