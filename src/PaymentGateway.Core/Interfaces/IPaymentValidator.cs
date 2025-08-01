using System.Threading.Tasks;

using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentValidator
{
    Task<ValidationResult> ValidateAsync(ProcessPaymentRequest request);
}