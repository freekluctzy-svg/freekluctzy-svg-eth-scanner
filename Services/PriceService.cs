using EthScanner.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EthScanner.Services;

public class PriceService : IPriceService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PriceSettings _settings;
    private readonly ILogger<PriceService> _logger;

    private decimal _cachedPrice;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PriceService(
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> options,
        ILogger<PriceService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("CoinGecko");
        _settings = options.Value.Price;
        _logger = logger;
    }

    public async Task<decimal> GetEthUsdPriceAsync(CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow < _cacheExpiry && _cachedPrice > 0)
            return _cachedPrice;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring the lock.
            if (DateTime.UtcNow < _cacheExpiry && _cachedPrice > 0)
                return _cachedPrice;

            var url = $"{_settings.CoinGeckoBaseUrl}/simple/price?ids=ethereum&vs_currencies=usd";
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var priceData = JsonConvert.DeserializeObject<CoinGeckoSimplePrice>(response);

            if (priceData?.Ethereum?.Usd > 0)
            {
                _cachedPrice = priceData.Ethereum.Usd;
                _cacheExpiry = DateTime.UtcNow.AddSeconds(_settings.CacheDurationSeconds);
                _logger.LogInformation("ETH/USD price updated: ${Price:N2}", _cachedPrice);
            }
            else
            {
                _logger.LogWarning("Failed to parse ETH/USD price from CoinGecko response.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ETH/USD price from CoinGecko.");
        }
        finally
        {
            _lock.Release();
        }

        return _cachedPrice;
    }

    public void Dispose() => _lock.Dispose();
}
