using Xunit;
using Moq;
using PaymentGateway.Core.Services;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using System;
using System.Threading.Tasks;

public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsSuccess_WhenValid()
    {
        var bankService = new Mock<IBankService>();
        bankService.Setup(b => b.ProcessPaymentAsync(It.IsAny<BankRequest>()))
            .ReturnsAsync(new BankResponse { IsSuccess = true, AuthorizationCode = "123" });

        var validator = new Mock<IPaymentValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(ValidationResult.Success());

        var repo = new Mock<IPaymentRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync(new Payment());

        var service = new PaymentService(repo.Object, bankService.Object, validator.Object);

        var request = new ProcessPaymentRequest { CardNumber = "12345678901234", Amount = 100, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" };
        var result = await service.ProcessPaymentAsync(request);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsFailure_WhenInvalid()
    {
        var bankService = new Mock<IBankService>();
        var validator = new Mock<IPaymentValidator>();
        var repo = new Mock<IPaymentRepository>();

        validator.Setup(v => v.ValidateAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(ValidationResult.Failure("CardNumber is required"));

        var service = new PaymentService(repo.Object, bankService.Object, validator.Object);

        var request = new ProcessPaymentRequest { CardNumber = "", Amount = 100, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" };
        var result = await service.ProcessPaymentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("CardNumber is required", result.ErrorMessage);
        Assert.Equal(PaymentStatus.Rejected, result.Status);
    }
    private static BankService CreateBankServiceStub()
    {
        var httpClient = new System.Net.Http.HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<PaymentGateway.Core.Services.BankService>();
        return new PaymentGateway.Core.Services.BankService(httpClient, logger);
    }

    [Fact]
    public async Task RefundPaymentAsync_ReturnsSuccess_WhenApproved()
    {
        var repo = new Mock<IPaymentRepository>();
        var bank = new Mock<IBankService>();
        var validator = new Mock<IPaymentValidator>();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Approved,
            Amount = 100,
            Currency = "USD",
            CardLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            AuthorizationCode = "auth"
        };
        repo.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        bank.Setup(b => b.RefundAsync(It.IsAny<BankRequest>())).ReturnsAsync(new BankResponse { IsSuccess = true });
        var service = new PaymentService(repo.Object, bank.Object, validator.Object);
        var result = await service.RefundPaymentAsync(payment.Id);
        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, result.Status);
    }

    [Fact]
    public async Task RefundPaymentAsync_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        var repo = new Mock<IPaymentRepository>();
        var bank = new Mock<IBankService>();
        var validator = new Mock<IPaymentValidator>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment)null);
        var service = new PaymentService(repo.Object, bank.Object, validator.Object);
        var result = await service.RefundPaymentAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RefundPaymentAsync_ReturnsRejected_WhenPaymentNotApproved()
    {
        var repo = new Mock<IPaymentRepository>();
        var bank = new Mock<IBankService>();
        var validator = new Mock<IPaymentValidator>();
        var payment = new Payment { Id = Guid.NewGuid(), Status = PaymentStatus.Declined };
        repo.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        var service = new PaymentService(repo.Object, bank.Object, validator.Object);
        var result = await service.RefundPaymentAsync(payment.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task RefundPaymentAsync_ReturnsRejected_WhenBankRefundFails()
    {
        var repo = new Mock<IPaymentRepository>();
        var bank = new Mock<IBankService>();
        var validator = new Mock<IPaymentValidator>();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Approved,
            Amount = 100,
            Currency = "USD",
            CardLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            AuthorizationCode = "auth"
        };
        repo.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        bank.Setup(b => b.RefundAsync(It.IsAny<BankRequest>())).ReturnsAsync(new BankResponse { IsSuccess = false, ErrorMessage = "Bank error" });
        var service = new PaymentService(repo.Object, bank.Object, validator.Object);
        var result = await service.RefundPaymentAsync(payment.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        Assert.Equal("Bank error", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessPaymentAsync_CallsRefund_WhenLocalSaveFails()
    {
        var bankService = new Mock<IBankService>();
        bankService.Setup(b => b.ProcessPaymentAsync(It.IsAny<BankRequest>()))
            .ReturnsAsync(new BankResponse { IsSuccess = true, AuthorizationCode = "auth123" });
        bankService.Setup(b => b.RefundPaymentAsync(It.IsAny<BankRequest>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var validator = new Mock<IPaymentValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(ValidationResult.Success());

        var repo = new Mock<IPaymentRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .ThrowsAsync(new Exception("DB error"));

        var service = new PaymentService(repo.Object, bankService.Object, validator.Object);
        var request = new ProcessPaymentRequest { CardNumber = "12345678901234", Amount = 100, Currency = "USD", ExpiryMonth = 12, ExpiryYear = 2025, Cvv = "123" };
        var result = await service.ProcessPaymentAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Error saving payment. Refund attempted.", result.ErrorMessage);
        bankService.Verify(b => b.RefundPaymentAsync(It.IsAny<BankRequest>(), "auth123"), Times.Once);
    }
}
