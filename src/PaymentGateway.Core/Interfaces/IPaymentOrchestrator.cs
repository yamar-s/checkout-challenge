using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentOrchestrator
{
    Task<ProcessPaymentResponse> ProcessAsync(ProcessPaymentRequest request);
    Task RecoverAsync(Guid transactionId);
}
