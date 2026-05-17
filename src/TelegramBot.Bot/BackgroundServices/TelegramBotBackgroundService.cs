using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBot.Bot.Services;

namespace TelegramBot.Bot.BackgroundServices;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramBotBackgroundService> _logger;

    public TelegramBotBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<TelegramBotBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram bot background service starting...");

        using var scope = _scopeFactory.CreateScope();
        var botService = scope.ServiceProvider.GetRequiredService<BotService>();
        await botService.StartAsync(stoppingToken);

        // Keep alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
