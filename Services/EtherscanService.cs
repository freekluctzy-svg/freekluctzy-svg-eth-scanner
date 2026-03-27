using EthScanner.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EthScanner.Services;

public class EtherscanService : IEtherscanService
{
    private readonly HttpClient _httpClient;
    private readonly EtherscanSettings _etherscanSettings;
    private readonly ScannerSettings _scannerSettings;
    private readonly IPriceService _priceService;
    private readonly ILogger<EtherscanService> _logger;

    // Track already-alerted transaction hashes to avoid duplicate notifications.
    // Bounded to prevent unbounded memory growth in long-running deployments.
    private readonly Queue<string> _alertedHashQueue = new();
    private readonly HashSet<string> _alertedHashes = new(StringComparer.OrdinalIgnoreCase);
    private const int MaxAlertedHashes = 10_000;

    // The last scanned block, used to avoid re-scanning old blocks.
    private long _lastScannedBlock = 0;

    public EtherscanService(
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> options,
        IPriceService priceService,
        ILogger<EtherscanService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Etherscan");
        _etherscanSettings = options.Value.Etherscan;
        _scannerSettings = options.Value.Scanner;
        _priceService = priceService;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionAlert>> ScanTransactionsAsync(
        CancellationToken cancellationToken = default)
    {
        var ethPrice = await _priceService.GetEthUsdPriceAsync(cancellationToken);
        if (ethPrice <= 0)
        {
            _logger.LogWarning("ETH price is unavailable. Skipping scan cycle.");
            return Enumerable.Empty<TransactionAlert>();
        }

        var alerts = new List<TransactionAlert>();

        _logger.LogInformation("Scanning transactions for addresses matching {Prefix}...{Suffix}",
            _etherscanSettings.AddressPrefix, _etherscanSettings.AddressSuffix);

        // Fetch transactions for each monitored address.
        foreach (var address in _etherscanSettings.MonitoredAddresses)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var transactions = await FetchTransactionsForAddressAsync(address, cancellationToken);
            foreach (var tx in transactions)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Skip already alerted transactions.
                if (_alertedHashes.Contains(tx.Hash))
                    continue;

                // Skip failed transactions.
                if (tx.IsError == "1")
                    continue;

                // Confirm at least one party matches the address pattern.
                if (!MatchesAddressPattern(tx.From) && !MatchesAddressPattern(tx.To))
                    continue;

                var ethAmount = tx.GetEthValue();
                var usdValue = ethAmount * ethPrice;

                if (usdValue < _scannerSettings.MinimumUsdValue)
                    continue;

                _alertedHashes.Add(tx.Hash);
                _alertedHashQueue.Enqueue(tx.Hash);

                // Evict oldest entries once the cap is reached.
                while (_alertedHashQueue.Count > MaxAlertedHashes)
                    _alertedHashes.Remove(_alertedHashQueue.Dequeue());

                alerts.Add(new TransactionAlert
                {
                    Hash = tx.Hash,
                    FromAddress = tx.From,
                    ToAddress = tx.To,
                    EthAmount = ethAmount,
                    UsdValue = usdValue,
                    EthUsdPrice = ethPrice,
                    Timestamp = tx.GetTimestamp(),
                    GasUsed = tx.GasUsed,
                    GasPriceGwei = tx.GetGasPriceGwei()
                });

                _logger.LogInformation(
                    "Alert: tx {Hash} | {Eth:F4} ETH (${Usd:N2}) | {From} -> {To}",
                    tx.Hash[..Math.Min(10, tx.Hash.Length)],
                    ethAmount, usdValue, tx.From, tx.To);
            }
        }

        return alerts;
    }

    private bool MatchesAddressPattern(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        return address.StartsWith(_etherscanSettings.AddressPrefix,
                   StringComparison.OrdinalIgnoreCase)
               && address.EndsWith(_etherscanSettings.AddressSuffix,
                   StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<EtherscanTransaction>> FetchTransactionsForAddressAsync(
        string address, CancellationToken cancellationToken)
    {
        var all = new List<EtherscanTransaction>();
        var startBlock = _lastScannedBlock > 0 ? _lastScannedBlock : 0;
        int page = 1;

        while (!cancellationToken.IsCancellationRequested)
        {
            var url = $"{_etherscanSettings.BaseUrl}" +
                      $"?module=account" +
                      $"&action=txlist" +
                      $"&address={address}" +
                      $"&startblock={startBlock}" +
                      $"&endblock=99999999" +
                      $"&page={page}" +
                      $"&offset={_scannerSettings.PageSize}" +
                      $"&sort=desc" +
                      $"&apikey={_etherscanSettings.ApiKey}";

            _logger.LogDebug("Fetching transactions for {Address} (page {Page})", address, page);

            try
            {
                var json = await _httpClient.GetStringAsync(url, cancellationToken);
                var response = JsonConvert
                    .DeserializeObject<EtherscanResponse<List<EtherscanTransaction>>>(json);

                if (response == null || !response.IsSuccess || response.Result == null || response.Result.Count == 0)
                {
                    if (response?.Message == "No transactions found")
                        _logger.LogDebug("No transactions found for {Address}.", address);
                    else if (response != null && !response.IsSuccess)
                        _logger.LogWarning("Etherscan API error for {Address}: {Message}", address, response.Message);
                    break;
                }

                all.AddRange(response.Result);

                // Track the latest block scanned.
                if (response.Result.Count > 0 &&
                    long.TryParse(response.Result[0].BlockNumber, out var latestBlock) &&
                    latestBlock > _lastScannedBlock)
                {
                    _lastScannedBlock = latestBlock;
                }

                // If fewer results than page size, we've reached the end.
                if (response.Result.Count < _scannerSettings.PageSize)
                    break;

                page++;

                // Etherscan free tier: max 5 API calls/second.
                await Task.Delay(250, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for {Address} (page {Page}).", address, page);
                break;
            }
        }

        return all;
    }
}
