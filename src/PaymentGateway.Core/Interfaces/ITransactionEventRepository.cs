using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionEventRepository
{
    Task SaveEventAsync(TransactionEvent transactionEvent);
    Task<List<TransactionEvent>> GetEventsByTransactionAsync(Guid transactionId);
    Task<TransactionEvent?> GetLastEventAsync(Guid transactionId);
}
