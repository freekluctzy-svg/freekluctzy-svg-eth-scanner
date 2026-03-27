namespace EthScanner.Models;

public class AppSettings
{
    public EtherscanSettings Etherscan { get; set; } = new();
    public DiscordSettings Discord { get; set; } = new();
    public PriceSettings Price { get; set; } = new();
    public ScannerSettings Scanner { get; set; } = new();
}

public class EtherscanSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.etherscan.io/api";

    /// <summary>Only addresses starting with this prefix are considered matching.</summary>
    public string AddressPrefix { get; set; } = "0x8f56";

    /// <summary>Only addresses ending with this suffix are considered matching.</summary>
    public string AddressSuffix { get; set; } = "dE7401";

    /// <summary>
    /// The Ethereum addresses to monitor. The scanner fetches all transactions for these
    /// addresses and alerts on any that match the prefix/suffix pattern and meet the USD
    /// threshold. Add the full Ethereum addresses you want to watch here.
    /// </summary>
    public List<string> MonitoredAddresses { get; set; } = new();
}

public class DiscordSettings
{
    /// <summary>Bot token for the Discord bot (from the Discord Developer Portal).</summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>The Discord channel ID where alerts will be posted.</summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Optional: Discord webhook URL (alternative to bot token + channel ID).</summary>
    public string WebhookUrl { get; set; } = string.Empty;
}

public class PriceSettings
{
    public string CoinGeckoBaseUrl { get; set; } = "https://api.coingecko.com/api/v3";

    /// <summary>How long (in seconds) to cache the ETH/USD price before refreshing.</summary>
    public int CacheDurationSeconds { get; set; } = 60;
}

public class ScannerSettings
{
    /// <summary>Minimum USD value for a transaction to trigger an alert.</summary>
    public decimal MinimumUsdValue { get; set; } = 10000;

    /// <summary>How often (in seconds) to poll Etherscan for new transactions.</summary>
    public int ScanIntervalSeconds { get; set; } = 60;

    /// <summary>Maximum number of transactions per Etherscan API page.</summary>
    public int PageSize { get; set; } = 100;
}
