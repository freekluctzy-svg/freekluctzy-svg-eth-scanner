using EthScanner.Models;

namespace EthScanner.Services;

public interface IEtherscanService
{
    /// <summary>
    /// Scans Etherscan for transactions involving addresses that start with the configured
    /// prefix and end with the configured suffix, returning those that exceed the USD threshold.
    /// </summary>
    Task<IEnumerable<TransactionAlert>> ScanTransactionsAsync(CancellationToken cancellationToken = default);
}
