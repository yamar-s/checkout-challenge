using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Repositories;

public class TransactionEventRepository : ITransactionEventRepository
{
    private readonly PaymentDbContext _context;

    public TransactionEventRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task SaveEventAsync(TransactionEvent transactionEvent)
    {
        _context.TransactionEvents.Add(transactionEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TransactionEvent>> GetEventsByTransactionAsync(Guid transactionId)
    {
        return await _context.TransactionEvents
            .Where(e => e.TransactionId == transactionId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<TransactionEvent?> GetLastEventAsync(Guid transactionId)
    {
        return await _context.TransactionEvents
            .Where(e => e.TransactionId == transactionId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }
}