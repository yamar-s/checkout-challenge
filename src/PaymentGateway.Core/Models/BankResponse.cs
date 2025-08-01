using System.Text.Json.Serialization;

namespace PaymentGateway.Core.Models;

public class BankResponse
{
    [JsonPropertyName("authorized")]
    public bool IsSuccess { get; set; }
    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
    public string? ErrorMessage { get; set; }
}