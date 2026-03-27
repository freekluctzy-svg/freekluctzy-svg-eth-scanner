using System.Net.Http.Headers;
using System.Text;
using EthScanner.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EthScanner.Services;

public class DiscordNotificationService : IDiscordNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly DiscordSettings _settings;
    private readonly ILogger<DiscordNotificationService> _logger;

    public DiscordNotificationService(
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> options,
        ILogger<DiscordNotificationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Discord");
        _settings = options.Value.Discord;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.BotToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bot", _settings.BotToken);
        }
    }

    public async Task SendTransactionAlertAsync(
        TransactionAlert alert, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.BotToken) && string.IsNullOrEmpty(_settings.WebhookUrl))
        {
            _logger.LogWarning("Discord is not configured. Set BotToken+ChannelId or WebhookUrl in appsettings.json.");
            return;
        }

        var embed = BuildEmbed(alert);
        var payload = new { embeds = new[] { embed } };
        var json = JsonConvert.SerializeObject(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response;

            if (!string.IsNullOrEmpty(_settings.WebhookUrl))
            {
                // Webhook approach (simpler, no bot token needed).
                response = await _httpClient.PostAsync(_settings.WebhookUrl, content, cancellationToken);
            }
            else
            {
                // Bot token approach: POST to the channel messages endpoint.
                var url = $"https://discord.com/api/v10/channels/{_settings.ChannelId}/messages";
                response = await _httpClient.PostAsync(url, content, cancellationToken);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Discord alert sent for tx {Hash}.", alert.Hash[..Math.Min(10, alert.Hash.Length)]);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Discord API returned {Status}: {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Discord alert for tx {Hash}.", alert.Hash);
        }
    }

    private static object BuildEmbed(TransactionAlert alert)
    {
        return new
        {
            title = "🚨 Large ETH Transaction Detected",
            color = 0xF6851B, // MetaMask orange
            url = alert.EtherscanUrl,
            fields = new[]
            {
                new
                {
                    name = "🔗 Transaction Hash",
                    value = $"[{alert.Hash[..Math.Min(20, alert.Hash.Length)]}...]({alert.EtherscanUrl})",
                    inline = false
                },
                new
                {
                    name = "📤 Sender",
                    value = $"[{alert.FromAddress}]({alert.SenderEtherscanUrl})",
                    inline = false
                },
                new
                {
                    name = "📥 Receiver",
                    value = $"[{alert.ToAddress}]({alert.ReceiverEtherscanUrl})",
                    inline = false
                },
                new
                {
                    name = "💰 Amount",
                    value = $"{alert.EthAmount:F4} ETH",
                    inline = true
                },
                new
                {
                    name = "💵 USD Value",
                    value = $"${alert.UsdValue:N2}",
                    inline = true
                },
                new
                {
                    name = "📈 ETH Price",
                    value = $"${alert.EthUsdPrice:N2}",
                    inline = true
                },
                new
                {
                    name = "⛽ Gas Used",
                    value = alert.GasUsed,
                    inline = true
                },
                new
                {
                    name = "⛽ Gas Price",
                    value = $"{alert.GasPriceGwei:F2} Gwei",
                    inline = true
                },
                new
                {
                    name = "🕐 Timestamp",
                    value = $"<t:{new DateTimeOffset(alert.Timestamp).ToUnixTimeSeconds()}:F>",
                    inline = true
                }
            },
            footer = new
            {
                text = "ETH Scanner | Powered by Etherscan & CoinGecko"
            },
            timestamp = alert.Timestamp.ToString("o")
        };
    }
}
