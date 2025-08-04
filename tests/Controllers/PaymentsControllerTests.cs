using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Enums;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Tests.Controllers
{
    public class PaymentsControllerTests
    {
        [Fact]
        public async Task ProcessPayment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var orchestratorMock = new Mock<IPaymentOrchestrator>();
            var serviceMock = new Mock<IPaymentService>();
            
            var response = new ProcessPaymentResponse
            {
                Id = Guid.NewGuid(),
                IsSuccess = true,
                Status = PaymentStatus.Authorized
            };
            
            orchestratorMock.Setup(x => x.ProcessAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(response);
            
            var controller = new PaymentsController(orchestratorMock.Object, serviceMock.Object);
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
            var result = await controller.ProcessPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProcessPaymentResponse>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsBadRequest_WhenRejected()
        {
            // Arrange
            var orchestratorMock = new Mock<IPaymentOrchestrator>();
            var serviceMock = new Mock<IPaymentService>();
            
            var response = new ProcessPaymentResponse
            {
                Id = Guid.NewGuid(),
                IsSuccess = false,
                Status = PaymentStatus.Rejected
            };
            
            orchestratorMock.Setup(x => x.ProcessAsync(It.IsAny<ProcessPaymentRequest>()))
                .ReturnsAsync(response);
            
            var controller = new PaymentsController(orchestratorMock.Object, serviceMock.Object);
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
            var result = await controller.ProcessPayment(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
