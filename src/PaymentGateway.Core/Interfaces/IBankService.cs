using PaymentGateway.Core.Models;
using System.Threading.Tasks;

namespace PaymentGateway.Core.Interfaces;

public interface IBankService
{
    Task<BankResponse> ProcessPaymentAsync(BankRequest request);
    Task RefundPaymentAsync(BankRequest request, string authorizationCode);
}