using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Handlers;

namespace TelegramBot.Bot.Services;

public class BotService
{
    private readonly ITelegramBotClient _bot;
    private readonly UpdateHandler _updateHandler;
    private readonly ILogger<BotService> _logger;

    public BotService(ITelegramBotClient bot, UpdateHandler updateHandler, ILogger<BotService> logger)
    {
        _bot = bot;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var me = await _bot.GetMe(ct);
        _logger.LogInformation("Bot started: @{Username}", me.Username);

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            },
            cancellationToken: ct);
    }

    private Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct) =>
        _updateHandler.HandleAsync(update, ct);

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException api => $"Telegram API Error [{api.ErrorCode}]: {api.Message}",
            _ => exception.ToString()
        };
        _logger.LogError("Bot error: {Error}", errorMessage);
        return Task.CompletedTask;
    }
}
