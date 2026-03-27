namespace EthScanner.Models;

/// <summary>Represents a processed transaction that has passed all filters and is ready for alerting.</summary>
public class TransactionAlert
{
    public string Hash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal EthAmount { get; set; }
    public decimal UsdValue { get; set; }
    public decimal EthUsdPrice { get; set; }
    public DateTime Timestamp { get; set; }
    public string GasUsed { get; set; } = string.Empty;
    public decimal GasPriceGwei { get; set; }
    public string EtherscanUrl => $"https://etherscan.io/tx/{Hash}";
    public string SenderEtherscanUrl => $"https://etherscan.io/address/{FromAddress}";
    public string ReceiverEtherscanUrl => $"https://etherscan.io/address/{ToAddress}";
}
