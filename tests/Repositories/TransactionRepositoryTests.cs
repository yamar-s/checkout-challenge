using Xunit;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Tests.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly PaymentDbContext _context;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new PaymentDbContext(options);
        _repository = new TransactionRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_SavesTransaction()
    {
        // Arrange
        var transaction = new PaymentTransaction
        {
            ClientRequestId = "test-123",
            Status = TransactionStatus.Created,
            Amount = 100,
            Currency = "USD"
        };

        // Act
        await _repository.CreateAsync(transaction);

        // Assert
        var saved = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(saved);
        Assert.Equal("test-123", saved.ClientRequestId);
        Assert.Equal(TransactionStatus.Created, saved.Status);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTransaction()
    {
        // Arrange
        var transaction = new PaymentTransaction
        {
            Status = TransactionStatus.Created,
            Amount = 100,
            Currency = "USD"
        };
        
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        transaction.Status = TransactionStatus.BankApproved;
        transaction.BankAuthCode = "AUTH123";
        await _repository.UpdateAsync(transaction);

        // Assert
        var updated = await _context.Transactions.FindAsync(transaction.Id);
        Assert.Equal(TransactionStatus.BankApproved, updated!.Status);
        Assert.Equal("AUTH123", updated.BankAuthCode);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTransaction_WhenExists()
    {
        // Arrange
        var transaction = new PaymentTransaction
        {
            Status = TransactionStatus.Created,
            Amount = 100,
            Currency = "USD"
        };
        
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(transaction.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveTransactions()
    {
        // Arrange
        var activeTransaction = new PaymentTransaction
        {
            Status = TransactionStatus.Created,
            Amount = 100,
            Currency = "USD"
        };
        
        var completedTransaction = new PaymentTransaction
        {
            Status = TransactionStatus.Completed,
            Amount = 200,
            Currency = "USD"
        };

        await _context.Transactions.AddRangeAsync(activeTransaction, completedTransaction);
        await _context.SaveChangesAsync();

        // Act
        var activeTransactions = await _repository.GetActiveAsync();

        // Assert
        Assert.Single(activeTransactions);
        Assert.Equal(activeTransaction.Id, activeTransactions[0].Id);
    }

    [Fact]
    public async Task GetFailedAsync_ReturnsOnlyFailedTransactions()
    {
        // Arrange
        var failedTransaction = new PaymentTransaction
        {
            Status = TransactionStatus.Failed,
            Amount = 100,
            Currency = "USD"
        };
        
        var reviewTransaction = new PaymentTransaction
        {
            Status = TransactionStatus.RequiresManualReview,
            Amount = 200,
            Currency = "USD"
        };
        
        var completedTransaction = new PaymentTransaction
        {
            Status = TransactionStatus.Completed,
            Amount = 300,
            Currency = "USD"
        };

        await _context.Transactions.AddRangeAsync(
            failedTransaction, reviewTransaction, completedTransaction);
        await _context.SaveChangesAsync();

        // Act
        var failedTransactions = await _repository.GetFailedAsync();

        // Assert
        Assert.Equal(1, failedTransactions.Count);
        Assert.Contains(failedTransactions, t => t.Id == failedTransaction.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
