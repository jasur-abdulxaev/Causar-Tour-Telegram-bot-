using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Bot.Services;

namespace TelegramBot.Bot.BackgroundServices;

public class SchedulerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulerBackgroundService> _logger;

    public SchedulerBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SchedulerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduler background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IAdvertisementRepository>();
                var publisher = scope.ServiceProvider.GetRequiredService<ChannelPublisherService>();

                var adsToPublish = await repo.GetAdsToPublishAsync(DateTime.UtcNow, stoppingToken);

                foreach (var ad in adsToPublish)
                {
                    _logger.LogInformation("Publishing scheduled ad {AdId}...", ad.Id);
                    await publisher.PublishAdAsync(ad.Id, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing scheduled ads.");
            }

            // Wait for 1 minute before checking again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
