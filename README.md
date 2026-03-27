# ETH Scanner — Etherscan + Discord Alert Bot

A C# .NET 8 console application that monitors Ethereum addresses matching a pattern (`0x8f56...dE7401`) for transactions exceeding $10,000 USD, and sends formatted alerts to a Discord channel.

## Features

- 🔍 **Address pattern filtering** — monitors addresses starting with `0x8f56` and ending with `dE7401`
- 💰 **USD threshold filtering** — only alerts on transactions above a configurable USD value (default: $10,000)
- 🔔 **Discord notifications** — rich embed messages with full transaction details
- 📈 **Real-time ETH/USD pricing** — via CoinGecko with configurable caching
- 🔄 **Continuous scanning** — configurable polling interval
- 📄 **Pagination support** — handles large transaction histories

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Etherscan API Key](https://etherscan.io/apis) (free tier works)
- Discord Bot Token **or** Discord Webhook URL

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/freekluctzy-svg/freekluctzy-svg-eth-scanner.git
cd freekluctzy-svg-eth-scanner
```

### 2. Get an Etherscan API Key

1. Go to [https://etherscan.io/register](https://etherscan.io/register) and create a free account
2. Navigate to **My Profile → API Keys**
3. Click **Add** to generate a new API key
4. Copy the key

### 3. Set up Discord (choose one option)

**Option A: Discord Bot Token (recommended)**
1. Go to [https://discord.com/developers/applications](https://discord.com/developers/applications)
2. Click **New Application**, give it a name
3. Navigate to **Bot** → **Add Bot**
4. Copy the **Bot Token**
5. Enable the bot in your server and copy the **Channel ID** (right-click channel → Copy ID)

**Option B: Discord Webhook (simpler)**
1. In your Discord server, go to **Channel Settings → Integrations → Webhooks**
2. Click **New Webhook**, configure it, then copy the **Webhook URL**

### 4. Configure `appsettings.json`

Open `appsettings.json` and fill in your values:

```json
{
  "AppSettings": {
    "Etherscan": {
      "ApiKey": "YOUR_ETHERSCAN_API_KEY",
      "AddressPrefix": "0x8f56",
      "AddressSuffix": "dE7401",
      "MonitoredAddresses": [
        "0x8f56YourFullEthereumAddressHeredE7401"
      ]
    },
    "Discord": {
      "BotToken": "YOUR_DISCORD_BOT_TOKEN",
      "ChannelId": "YOUR_DISCORD_CHANNEL_ID",
      "WebhookUrl": ""
    },
    "Scanner": {
      "MinimumUsdValue": 10000,
      "ScanIntervalSeconds": 60
    }
  }
}
```

> **Note:** Add all Ethereum addresses you want to monitor to `MonitoredAddresses`. Each address will be checked against the prefix/suffix pattern before alerting.

### 5. Run in Visual Studio

1. Open `eth-scanner.csproj` in Visual Studio 2022+
2. Restore NuGet packages (right-click solution → **Restore NuGet Packages**)
3. Press **F5** to run

### 6. Run from command line

```bash
dotnet restore
dotnet run
```

## Configuration Reference

| Setting | Description | Default |
|---------|-------------|---------|
| `Etherscan.ApiKey` | Your Etherscan API key | _(required)_ |
| `Etherscan.AddressPrefix` | Address prefix filter | `0x8f56` |
| `Etherscan.AddressSuffix` | Address suffix filter | `dE7401` |
| `Etherscan.MonitoredAddresses` | List of addresses to monitor | _(required)_ |
| `Discord.BotToken` | Discord bot token | _(required if no webhook)_ |
| `Discord.ChannelId` | Discord channel ID | _(required with bot token)_ |
| `Discord.WebhookUrl` | Discord webhook URL | _(alternative to bot token)_ |
| `Price.CacheDurationSeconds` | ETH price cache TTL in seconds | `60` |
| `Scanner.MinimumUsdValue` | Minimum USD value for alerts | `10000` |
| `Scanner.ScanIntervalSeconds` | Seconds between scans | `60` |
| `Scanner.PageSize` | Transactions per Etherscan API page | `100` |

## Discord Alert Format

Each alert includes:
- 🔗 Transaction hash with Etherscan link
- 📤 Sender address with Etherscan link
- 📥 Receiver address with Etherscan link
- 💰 ETH amount
- 💵 USD value at time of detection
- 📈 ETH/USD price used
- ⛽ Gas used and gas price
- 🕐 Transaction timestamp

## Architecture

```
eth-scanner/
├── Program.cs                         # Entry point, DI setup
├── eth-scanner.csproj                 # Project file
├── appsettings.json                   # Configuration
├── Models/
│   ├── AppConfiguration.cs            # Settings models
│   ├── EtherscanModels.cs             # API response models
│   └── TransactionAlert.cs            # Alert data model
└── Services/
    ├── IEtherscanService.cs           # Scanner interface
    ├── EtherscanService.cs            # Etherscan API implementation
    ├── IPriceService.cs               # Price service interface
    ├── PriceService.cs                # CoinGecko price implementation
    ├── IDiscordNotificationService.cs # Discord interface
    ├── DiscordNotificationService.cs  # Discord API implementation
    └── ScannerBackgroundService.cs    # Background scanning loop
```

## License

MIT
