using Xunit;
using Moq;
using PaymentGateway.Core.Services;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Tests.Services
{
    public class PaymentServiceTests
    {
        [Fact]
        public async Task ProcessPaymentAsync_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var repo = new Mock<IPaymentRepository>();
            var bank = new Mock<IBankService>();
            var validator = new Mock<IPaymentValidator>();
            
            validator.Setup(v => v.ValidateAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(new ValidationResult());
                
            bank.Setup(b => b.ProcessPaymentAsync(It.IsAny<BankRequest>()))
                .ReturnsAsync(new BankResponse { IsSuccess = true, AuthorizationCode = "AUTH123" });
                
            repo.Setup(r => r.AddAsync(It.IsAny<Payment>()))
                .ReturnsAsync(new Payment { Id = Guid.NewGuid() });

            var service = new PaymentService(repo.Object, bank.Object, validator.Object);
            var request = new ProcessPaymentRequest
            {
                ClientRequestId = "test-123",
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Currency = "USD",
                Amount = 100,
                Cvv = "123"
            };

            // Act
            var result = await service.ProcessPaymentAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentStatus.Authorized, result.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ReturnsRejected_WhenInvalid()
        {
            // Arrange
            var repo = new Mock<IPaymentRepository>();
            var bank = new Mock<IBankService>();
            var validator = new Mock<IPaymentValidator>();
            
            var validationResult = new ValidationResult();
            validationResult.AddError("CardNumber is required");
            validator.Setup(v => v.ValidateAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(validationResult);

            var service = new PaymentService(repo.Object, bank.Object, validator.Object);
            var request = new ProcessPaymentRequest
            {
                ClientRequestId = "test-123",
                CardNumber = "",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Currency = "USD",
                Amount = 100,
                Cvv = "123"
            };

            // Act
            var result = await service.ProcessPaymentAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Rejected, result.Status);
        }

        [Fact]
        public async Task GetPaymentAsync_ReturnsPayment_WhenExists()
        {
            // Arrange
            var repo = new Mock<IPaymentRepository>();
            var bank = new Mock<IBankService>();
            var validator = new Mock<IPaymentValidator>();
            
            var payment = new Payment { Id = Guid.NewGuid(), Status = PaymentStatus.Authorized };
            repo.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);

            var service = new PaymentService(repo.Object, bank.Object, validator.Object);

            // Act
            var result = await service.GetPaymentAsync(payment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.Id, result.Id);
        }

        [Fact]
        public async Task GetPaymentAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var repo = new Mock<IPaymentRepository>();
            var bank = new Mock<IBankService>();
            var validator = new Mock<IPaymentValidator>();
            
            repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment)null);

            var service = new PaymentService(repo.Object, bank.Object, validator.Object);

            // Act
            var result = await service.GetPaymentAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
    }
}
