using Newtonsoft.Json;

namespace EthScanner.Models;

public class EtherscanResponse<T>
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("result")]
    public T? Result { get; set; }

    public bool IsSuccess => Status == "1";
}

public class EtherscanTransaction
{
    [JsonProperty("blockNumber")]
    public string BlockNumber { get; set; } = string.Empty;

    [JsonProperty("timeStamp")]
    public string TimeStamp { get; set; } = string.Empty;

    [JsonProperty("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonProperty("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [JsonProperty("blockHash")]
    public string BlockHash { get; set; } = string.Empty;

    [JsonProperty("transactionIndex")]
    public string TransactionIndex { get; set; } = string.Empty;

    [JsonProperty("from")]
    public string From { get; set; } = string.Empty;

    [JsonProperty("to")]
    public string To { get; set; } = string.Empty;

    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;

    [JsonProperty("gas")]
    public string Gas { get; set; } = string.Empty;

    [JsonProperty("gasPrice")]
    public string GasPrice { get; set; } = string.Empty;

    [JsonProperty("isError")]
    public string IsError { get; set; } = string.Empty;

    [JsonProperty("txreceipt_status")]
    public string TxReceiptStatus { get; set; } = string.Empty;

    [JsonProperty("input")]
    public string Input { get; set; } = string.Empty;

    [JsonProperty("contractAddress")]
    public string ContractAddress { get; set; } = string.Empty;

    [JsonProperty("cumulativeGasUsed")]
    public string CumulativeGasUsed { get; set; } = string.Empty;

    [JsonProperty("gasUsed")]
    public string GasUsed { get; set; } = string.Empty;

    [JsonProperty("confirmations")]
    public string Confirmations { get; set; } = string.Empty;

    /// <summary>Gets the ETH value from Wei (1 ETH = 10^18 Wei).</summary>
    public decimal GetEthValue()
    {
        if (decimal.TryParse(Value, out var wei))
            return wei / 1_000_000_000_000_000_000m;
        return 0m;
    }

    /// <summary>Gets the transaction timestamp as a UTC DateTime.</summary>
    public DateTime GetTimestamp()
    {
        if (long.TryParse(TimeStamp, out var unixTime))
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        return DateTime.UtcNow;
    }

    /// <summary>Gets the gas price in Gwei.</summary>
    public decimal GetGasPriceGwei()
    {
        if (decimal.TryParse(GasPrice, out var wei))
            return wei / 1_000_000_000m;
        return 0m;
    }
}

public class CoinGeckoSimplePrice
{
    [JsonProperty("ethereum")]
    public EthereumPrice? Ethereum { get; set; }
}

public class EthereumPrice
{
    [JsonProperty("usd")]
    public decimal Usd { get; set; }
}
