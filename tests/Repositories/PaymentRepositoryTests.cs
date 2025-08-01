using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task AddAsync_SavesAndRetrievesPayment()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new PaymentDbContext(options);
        var logger = new Mock<ILogger<PaymentRepository>>().Object;
        var repo = new PaymentRepository(context, logger);

        var payment = new Payment
        {
            Amount = 100,
            Currency = "USD",
            Status = PaymentGateway.Core.Enums.PaymentStatus.Authorized,
            CardLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            AuthorizationCode = "AUTH123"
        };

        var saved = await repo.AddAsync(payment);
        var found = await repo.GetByIdAsync(saved.Id);

        Assert.NotNull(found);
        Assert.Equal(100, found.Amount);
        Assert.Equal("USD", found.Currency);
        Assert.Equal("1234", found.CardLastFour);
    }

    [Fact]
    public async Task AddAsync_ThrowsException_WhenDbFails()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new PaymentDbContext(options);
        var logger = new Mock<ILogger<PaymentRepository>>().Object;
        var repo = new PaymentRepository(context, logger);

        context.Dispose(); 

        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await repo.AddAsync(new Payment());
        });
    }
}
