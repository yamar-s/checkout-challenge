using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface ITransactionRepository
{
    Task CreateAsync(PaymentTransaction transaction);
    Task UpdateAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetByIdAsync(Guid id);
    Task<List<PaymentTransaction>> GetActiveAsync();
    Task<List<PaymentTransaction>> GetFailedAsync();
}
