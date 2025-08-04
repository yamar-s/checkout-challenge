using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Services;

public class TransactionRecoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionRecoveryService> _logger;

    public TransactionRecoveryService(
        IServiceProvider serviceProvider,
        ILogger<TransactionRecoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IPaymentOrchestrator>();

                var failedTransactions = await transactionRepository.GetFailedAsync();
                
                foreach (var transaction in failedTransactions)
                {
                    try
                    {
                        _logger.LogInformation("Attempting recovery for transaction {TransactionId}", transaction.Id);
                        await orchestrator.RecoverAsync(transaction.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to recover transaction {TransactionId}", transaction.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction recovery service");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}