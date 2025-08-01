using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Mappers;
using PaymentGateway.Core.Models;
using PaymentGateway.Core.Utils;

namespace PaymentGateway.Core.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IBankService _bankService;
    private readonly IPaymentValidator _validator;

    public PaymentService(
        IPaymentRepository repository,
        IBankService bankService,
        IPaymentValidator validator)
    {
        _repository = repository;
        _bankService = bankService;
        _validator = validator;
    }

    private ProcessPaymentResponse CreateResponse(bool isSuccess, List<string>? errors, PaymentStatus status)
    {
        return new ProcessPaymentResponse
        {
            IsSuccess = isSuccess,
            ErrorMessages = errors,
            Status = status
        };
    }

    public async Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return CreateResponse(false, validationResult.Errors, PaymentStatus.Rejected);
        }

        var bankRequest = PaymentMapper.ToBankRequest(request);
        var bankResponse = await _bankService.ProcessPaymentAsync(bankRequest);

        if (!bankResponse.IsSuccess)
            return CreateResponse(false, bankResponse.ErrorMessage != null ? new List<string> { bankResponse.ErrorMessage } : null, PaymentStatus.Declined);

        var payment = PaymentMapper.ToPaymentEntity(request, bankResponse);
        payment.CardLastFour = CardUtils.GetLastFourDigits(request.CardNumber);
        payment.Status = PaymentStatus.Authorized;

        try
        {
            await _repository.AddAsync(payment);
            return new ProcessPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount,
                CardLastFour = payment.CardLastFour,
                IsSuccess = true,
                AuthorizationCode = payment.AuthorizationCode
            };
        }
        catch (Exception ex)
        {
            try
            {
                await _bankService.RefundPaymentAsync(bankRequest, bankResponse.AuthorizationCode);
            }
            catch (Exception refundEx)
            {
                Console.Error.WriteLine($"[Refund] Failed to refund after local save error: {refundEx.Message}");
            }
            return CreateResponse(false, new List<string> { "Error saving payment. Refund attempted." }, PaymentStatus.Failed);
        }
    }

    public async Task<Payment?> GetPaymentAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}