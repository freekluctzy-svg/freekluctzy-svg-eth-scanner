namespace EthScanner.Services;

public interface IPriceService
{
    /// <summary>Returns the current ETH/USD price, using cache when available.</summary>
    Task<decimal> GetEthUsdPriceAsync(CancellationToken cancellationToken = default);
}
