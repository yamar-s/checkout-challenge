using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PaymentStatus Status { get; set; }

    public string CardLastFour { get; set; } = string.Empty;

    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AuthorizationCode { get; set; }
}