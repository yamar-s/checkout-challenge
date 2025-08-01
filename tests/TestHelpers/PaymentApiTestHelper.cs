using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PaymentGateway.Tests.TestHelpers
{
    public static class PaymentApiTestHelper
    {
        public static object ValidPaymentPayload() => new
        {
            CardNumber = "12345678901231",
            Amount = 100,
            Currency = "USD",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Cvv = "123"
        };

        public static IEnumerable<object[]> InvalidPaymentPayloads()
        {
            yield return new object[] { new { CardNumber = "", Amount = 100, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" } };
            yield return new object[] { new { CardNumber = "12345678901231", Amount = 0, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" } };
            yield return new object[] { new { CardNumber = "12345678901231", Amount = 100, Currency = "", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" } };
            yield return new object[] { new { CardNumber = "12345678901231", Amount = 100, Currency = "USD", ExpiryMonth = 0, ExpiryYear = 2025, Cvv = "123" } };
            yield return new object[] { new { CardNumber = "12345678901231", Amount = 100, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "" } };
        }

        public static StringContent ToJsonContent(object payload) =>
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        public static async Task<HttpResponseMessage> PostPaymentAsync(HttpClient client, object payload)
        {
            return await client.PostAsync("/api/payments", ToJsonContent(payload));
        }

        public static async Task<Guid> PostPaymentAndGetIdAsync(HttpClient client, object payload)
        {
            var response = await PostPaymentAsync(client, payload);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);
            var id = result.TryGetProperty("id", out var idProp) ? idProp.GetGuid() : Guid.Empty;
            if (id == Guid.Empty && result.TryGetProperty("paymentId", out var paymentIdProp))
                id = paymentIdProp.GetGuid();
            return id;
        }

        public static async Task<HttpResponseMessage> GetPaymentAsync(HttpClient client, Guid id)
        {
            return await client.GetAsync($"/api/payments/{id}");
        }
    }
}
