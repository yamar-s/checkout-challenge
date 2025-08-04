using Xunit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Core.Entities;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Tests.Repositories
{
    public class TransactionEventRepositoryTests : IDisposable
    {
        private readonly PaymentDbContext _context;
        private readonly TransactionEventRepository _repository;

        public TransactionEventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentDbContext(options);
            _repository = new TransactionEventRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_CreatesTransactionEvent()
        {
            // Arrange
            var transactionEvent = new TransactionEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                EventType = "TestEvent",
                Description = "Test event description",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.SaveEventAsync(transactionEvent);

            // Assert
            var saved = await _context.TransactionEvents.FindAsync(transactionEvent.Id);
            Assert.NotNull(saved);
            Assert.Equal("TestEvent", saved.EventType);
            Assert.Equal("Test event description", saved.Description);
        }

        [Fact]
        public async Task GetEventsByTransactionAsync_ReturnsEvents()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var event1 = new TransactionEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                EventType = "Event1",
                Description = "First event",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };
            var event2 = new TransactionEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                EventType = "Event2",
                Description = "Second event",
                CreatedAt = DateTime.UtcNow
            };

            await _context.TransactionEvents.AddRangeAsync(event1, event2);
            await _context.SaveChangesAsync();

            // Act
            var events = await _repository.GetEventsByTransactionAsync(transactionId);

            // Assert
            Assert.Equal(2, events.Count);
            Assert.Contains(events, e => e.EventType == "Event1");
            Assert.Contains(events, e => e.EventType == "Event2");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
