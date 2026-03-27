using EthScanner.Models;

namespace EthScanner.Services;

public interface IDiscordNotificationService
{
    /// <summary>Sends a transaction alert to the configured Discord channel.</summary>
    Task SendTransactionAlertAsync(TransactionAlert alert, CancellationToken cancellationToken = default);
}
