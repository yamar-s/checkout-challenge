using System;
using System.Threading.Tasks;

using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentService
{
    Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<Payment?> GetPaymentAsync(Guid id);
}