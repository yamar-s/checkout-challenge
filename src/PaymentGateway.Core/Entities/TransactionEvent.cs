namespace PaymentGateway.Core.Entities;

public class TransactionEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? EventData { get; set; }
    
    public PaymentTransaction? Transaction { get; set; }
}
