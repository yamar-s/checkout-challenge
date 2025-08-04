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
    public class PaymentOrchestratorTests
    {
        [Fact]
        public async Task ProcessAsync_ReturnsSuccess_WhenPaymentSucceeds()
        {
            // Arrange
            var paymentService = new Mock<IPaymentService>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var eventRepo = new Mock<ITransactionEventRepository>();

            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(new ProcessPaymentResponse 
                { 
                    Id = Guid.NewGuid(), 
                    IsSuccess = true, 
                    Status = PaymentStatus.Authorized 
                });

            var orchestrator = new PaymentOrchestrator(paymentService.Object, transactionRepo.Object, eventRepo.Object);
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
            var result = await orchestrator.ProcessAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentStatus.Authorized, result.Status);
            transactionRepo.Verify(r => r.CreateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            transactionRepo.Verify(r => r.UpdateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            eventRepo.Verify(r => r.SaveEventAsync(It.IsAny<TransactionEvent>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessAsync_ReturnsFailed_WhenPaymentFails()
        {
            // Arrange
            var paymentService = new Mock<IPaymentService>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var eventRepo = new Mock<ITransactionEventRepository>();

            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(new ProcessPaymentResponse 
                { 
                    Id = Guid.NewGuid(), 
                    IsSuccess = false, 
                    Status = PaymentStatus.Rejected 
                });

            var orchestrator = new PaymentOrchestrator(paymentService.Object, transactionRepo.Object, eventRepo.Object);
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
            var result = await orchestrator.ProcessAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Rejected, result.Status);
            transactionRepo.Verify(r => r.CreateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            transactionRepo.Verify(r => r.UpdateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            eventRepo.Verify(r => r.SaveEventAsync(It.IsAny<TransactionEvent>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessAsync_HandlesExceptions()
        {
            // Arrange
            var paymentService = new Mock<IPaymentService>();
            var transactionRepo = new Mock<ITransactionRepository>();
            var eventRepo = new Mock<ITransactionEventRepository>();

            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
                .ThrowsAsync(new Exception("Bank service unavailable"));

            var orchestrator = new PaymentOrchestrator(paymentService.Object, transactionRepo.Object, eventRepo.Object);
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

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => orchestrator.ProcessAsync(request));
            
            transactionRepo.Verify(r => r.CreateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            transactionRepo.Verify(r => r.UpdateAsync(It.IsAny<PaymentTransaction>()), Times.Once);
            eventRepo.Verify(r => r.SaveEventAsync(It.IsAny<TransactionEvent>()), Times.AtLeastOnce);
        }
    }
}
