using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Core.Services;

public class PaymentOrchestrator : IPaymentOrchestrator
{
    private readonly IPaymentService _paymentService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionEventRepository _eventRepository;

    public PaymentOrchestrator(
        IPaymentService paymentService,
        ITransactionRepository transactionRepository,
        ITransactionEventRepository eventRepository)
    {
        _paymentService = paymentService;
        _transactionRepository = transactionRepository;
        _eventRepository = eventRepository;
    }

    public async Task<ProcessPaymentResponse> ProcessAsync(ProcessPaymentRequest request)
    {
        var transaction = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            ClientRequestId = request.ClientRequestId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = TransactionStatus.Started,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        await _transactionRepository.CreateAsync(transaction);
        await LogEventAsync(transaction.Id, "TransactionStarted", "Payment transaction initiated");

        try
        {
            var result = await _paymentService.ProcessPaymentAsync(request);
            
            transaction.PaymentId = result.Id ?? transaction.PaymentId;
            transaction.Status = result.IsSuccess ? TransactionStatus.Completed : TransactionStatus.Failed;
            transaction.LastUpdated = DateTime.UtcNow;
            
            await _transactionRepository.UpdateAsync(transaction);
            await LogEventAsync(transaction.Id, result.IsSuccess ? "PaymentSucceeded" : "PaymentFailed", 
                result.IsSuccess ? "Payment processed successfully" : $"Payment failed: {string.Join(", ", result.ErrorMessages ?? new List<string>())}");

            return result;
        }
        catch (Exception ex)
        {
            transaction.Status = TransactionStatus.Failed;
            transaction.LastUpdated = DateTime.UtcNow;
            
            await _transactionRepository.UpdateAsync(transaction);
            await LogEventAsync(transaction.Id, "PaymentError", $"Payment processing error: {ex.Message}");
            
            throw;
        }
    }

    public async Task RecoverAsync(Guid transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return;

        await LogEventAsync(transactionId, "RecoveryStarted", "Transaction recovery initiated");

        if (transaction.Status == TransactionStatus.Started || transaction.Status == TransactionStatus.Failed)
        {
            transaction.Status = TransactionStatus.Failed;
            transaction.LastUpdated = DateTime.UtcNow;
            await _transactionRepository.UpdateAsync(transaction);
            await LogEventAsync(transactionId, "RecoveryCompleted", "Transaction marked as failed during recovery");
        }
    }

    private async Task LogEventAsync(Guid transactionId, string eventType, string description)
    {
        var transactionEvent = new TransactionEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            EventType = eventType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _eventRepository.SaveEventAsync(transactionEvent);
    }
}