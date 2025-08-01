using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Entities;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentService
{
    Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<Payment?> GetPaymentAsync(Guid id);
}