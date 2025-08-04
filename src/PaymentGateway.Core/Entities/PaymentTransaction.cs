using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public enum TransactionStatus
{
    Started,
    Created,
    BankApproved,
    Saved,
    Completed,
    Refunding,
    Failed,
    RequiresManualReview
}

public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaymentId { get; set; }
    public string? ClientRequestId { get; set; }
    public TransactionStatus Status { get; set; }
    public string? BankAuthCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? LastError { get; set; }
    public int RetryAttempts { get; set; }
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
