using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionRecoveryService
{
    Task RecoverFailedTransactionsAsync();
    Task<List<TransactionEvent>> GetTransactionTimelineAsync(Guid transactionId);
    Task<List<PaymentTransaction>> GetActiveTransactionsAsync();
}
