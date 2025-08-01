using System.Threading.Tasks;

using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Interfaces;

public interface IBankService
{
    Task<BankResponse> ProcessPaymentAsync(BankRequest request);
    Task RefundPaymentAsync(BankRequest request, string authorizationCode);
}