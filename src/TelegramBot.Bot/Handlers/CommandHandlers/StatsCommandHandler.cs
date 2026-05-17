using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Services;

namespace TelegramBot.Bot.Handlers.CommandHandlers;

public class StatsCommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IAnalyticsService _analyticsService;

    public StatsCommandHandler(ITelegramBotClient bot, IAnalyticsService analyticsService)
    {
        _bot = bot;
        _analyticsService = analyticsService;
    }

    public async Task HandleAsync(long chatId, long adminId, CancellationToken ct)
    {
        var stats = await _analyticsService.GetAllSummaryAsync(ct);

        if (!stats.Any())
        {
            await _bot.SendMessage(chatId, "Hali statistika ma'lumotlari yo'q.", cancellationToken: ct);
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("📊 *Barcha e'lonlar statistikasi:*\n");

        foreach (var s in stats)
        {
            sb.AppendLine(MessageFormatterService.FormatAdminStats(s));
            sb.AppendLine("─────────────────");
        }

        await _bot.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html, cancellationToken: ct);
    }
}
