using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBot.Application;
using TelegramBot.Bot;
using TelegramBot.Bot.BackgroundServices;
using TelegramBot.Bot.Handlers;
using TelegramBot.Bot.Handlers.CallbackHandlers;
using TelegramBot.Bot.Handlers.CommandHandlers;
using TelegramBot.Bot.Handlers.MessageHandlers;
using TelegramBot.Bot.Services;
using TelegramBot.Infrastructure;
using TelegramBot.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

// Settings
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection("BotSettings"));

// Infrastructure + Application
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// Telegram Bot Client
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<BotSettings>>().Value;
    if (string.IsNullOrWhiteSpace(settings.BotToken))
        throw new InvalidOperationException("BotSettings:BotToken is not configured.");
    return new TelegramBotClient(settings.BotToken);
});

// Handlers (scoped because they use scoped services via IServiceProvider or directly injected)
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<StartCommandHandler>();
builder.Services.AddScoped<CreateAdCommandHandler>();
builder.Services.AddScoped<MyAdsCommandHandler>();
builder.Services.AddScoped<StatsCommandHandler>();
builder.Services.AddScoped<TextMessageHandler>();
builder.Services.AddScoped<PhotoMessageHandler>();
builder.Services.AddScoped<InterestCallbackHandler>();
builder.Services.AddScoped<AdminActionCallbackHandler>();

// Bot orchestration
builder.Services.AddScoped<ChannelPublisherService>();
builder.Services.AddScoped<BotSetupService>();
builder.Services.AddScoped<BotService>();
builder.Services.AddHostedService<TelegramBotBackgroundService>();
builder.Services.AddHostedService<SchedulerBackgroundService>();
builder.Services.AddHostedService<UserBroadcastBackgroundService>();
builder.Services.AddHostedService<DatabaseMaintenanceBackgroundService>();

var host = builder.Build();

// Init DB
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await db.InitializeAsync();
}

// Bot description va buyruqlarni o'rnatamiz
using (var scope = host.Services.CreateScope())
{
    var setup = scope.ServiceProvider.GetRequiredService<BotSetupService>();
    await setup.SetupAsync();
}

await host.RunAsync();
