using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PaymentDbContext _context;

    public TransactionRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(PaymentTransaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PaymentTransaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<List<PaymentTransaction>> GetActiveAsync()
    {
        return await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Started || t.Status == TransactionStatus.Created)
            .ToListAsync();
    }

    public async Task<List<PaymentTransaction>> GetFailedAsync()
    {
        return await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Failed)
            .ToListAsync();
    }
}