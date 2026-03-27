using EthScanner.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EthScanner.Services;

/// <summary>
/// A long-running background service that continuously scans Etherscan for matching
/// transactions and sends Discord alerts.
/// </summary>
public class ScannerBackgroundService : BackgroundService
{
    private readonly IEtherscanService _etherscanService;
    private readonly IDiscordNotificationService _discordService;
    private readonly ScannerSettings _scannerSettings;
    private readonly ILogger<ScannerBackgroundService> _logger;

    public ScannerBackgroundService(
        IEtherscanService etherscanService,
        IDiscordNotificationService discordService,
        IOptions<AppSettings> options,
        ILogger<ScannerBackgroundService> logger)
    {
        _etherscanService = etherscanService;
        _discordService = discordService;
        _scannerSettings = options.Value.Scanner;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ETH Scanner started. Scan interval: {Interval}s. Minimum USD threshold: ${Threshold:N0}.",
            _scannerSettings.ScanIntervalSeconds,
            _scannerSettings.MinimumUsdValue);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var alerts = await _etherscanService.ScanTransactionsAsync(stoppingToken);

                foreach (var alert in alerts)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    await _discordService.SendTransactionAlertAsync(alert, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during scan cycle.");
            }

            _logger.LogInformation(
                "Scan cycle complete. Next scan in {Interval}s.",
                _scannerSettings.ScanIntervalSeconds);

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(_scannerSettings.ScanIntervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("ETH Scanner stopped.");
    }
}
