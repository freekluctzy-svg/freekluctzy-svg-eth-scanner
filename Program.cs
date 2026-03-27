using EthScanner.Models;
using EthScanner.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Bind configuration.
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

        // Register named HttpClients.
        services.AddHttpClient("Etherscan", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient("CoinGecko", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient("Discord", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Register services.
        services.AddSingleton<IPriceService, PriceService>();
        services.AddSingleton<IEtherscanService, EtherscanService>();
        services.AddSingleton<IDiscordNotificationService, DiscordNotificationService>();

        // Register the background scanner.
        services.AddHostedService<ScannerBackgroundService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

Console.WriteLine("========================================");
Console.WriteLine("  ETH Scanner - Etherscan + Discord     ");
Console.WriteLine("========================================");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

await host.RunAsync();
