using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Extensions;

public static class TransactionExtensions
{
    public static async Task UpdateStatusAsync(this PaymentTransaction transaction, 
        TransactionStatus status, 
        ITransactionRepository repository, 
        string? error = null)
    {
        transaction.Status = status;
        if (!string.IsNullOrEmpty(error))
        {
            transaction.LastError = error;
        }
        
        await repository.UpdateAsync(transaction);
    }
}
