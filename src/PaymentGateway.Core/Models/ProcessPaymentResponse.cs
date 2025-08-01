using PaymentGateway.Core.Enums;
using System.Collections.Generic;

namespace PaymentGateway.Core.Models;

public class ProcessPaymentResponse
{
    public Guid? Id { get; set; }
    public PaymentStatus Status { get; set; }       
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string CardLastFour { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? AuthorizationCode { get; set; }
    public List<string>? ErrorMessages { get; set; } // Changed from single string
}