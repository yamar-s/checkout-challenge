using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllAsync();
}