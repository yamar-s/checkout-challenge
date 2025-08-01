using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;
using System.Threading.Tasks;

namespace PaymentGateway.Core.Interfaces;

public interface IPaymentValidator
{
    Task<ValidationResult> ValidateAsync(ProcessPaymentRequest request);
}