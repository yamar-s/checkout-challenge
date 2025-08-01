using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Services;

public class BankService : IBankService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankService> _logger;

    public BankService(HttpClient httpClient, ILogger<BankService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BankResponse> ProcessPaymentAsync(BankRequest request)
    {
        try
        {
            var content = JsonContent.Create(request);
            var response = await _httpClient.PostAsync("/payments", content);
            response.EnsureSuccessStatusCode();
            var rawJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BankResponse>(rawJson);
        }
        catch (Exception ex)
        {
            return new BankResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task RefundPaymentAsync(BankRequest request, string authorizationCode)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/payments/{authorizationCode}");
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Refunded payment with authorization code {authorizationCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refund payment with authorization code {authorizationCode}");
            throw;
        }
    }
}