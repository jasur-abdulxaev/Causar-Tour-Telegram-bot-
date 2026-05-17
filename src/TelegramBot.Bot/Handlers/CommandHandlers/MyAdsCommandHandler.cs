using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Keyboards;
using TelegramBot.Bot.Services;

namespace TelegramBot.Bot.Handlers.CommandHandlers;

public class MyAdsCommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IAdvertisementService _adService;

    public MyAdsCommandHandler(ITelegramBotClient bot, IAdvertisementService adService)
    {
        _bot = bot;
        _adService = adService;
    }

    public async Task HandleAsync(long chatId, long adminId, int page = 1, int? messageId = null, CancellationToken ct = default)
    {
        var ads = await _adService.GetByAdminAsync(adminId, ct);

        if (!ads.Any())
        {
            var text = "Siz hali e'lon yaratmadingiz. /create_ad buyrug'ini ishlatib yangi e'lon yarating.";
            if (messageId.HasValue)
                await _bot.EditMessageText(chatId, messageId.Value, text, cancellationToken: ct);
            else
                await _bot.SendMessage(chatId, text, cancellationToken: ct);
            return;
        }

        int pageSize = 8;
        int totalAds = ads.Count;
        int totalPages = (int)Math.Ceiling(totalAds / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages));

        var pageAds = ads.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📋 <b>Mening e'lonlarim ({page}-{totalPages} sahifa)</b>");
        sb.AppendLine($"Jami: {totalAds} ta e'lon\n");

        for (int i = 0; i < pageAds.Count; i++)
        {
            var ad = pageAds[i];
            int displayIndex = (page - 1) * pageSize + i + 1;
            
            string status = ad.IsPublished ? "✅ Yuborilgan" 
                : (ad.ScheduledPublishTime.HasValue ? $"⏰ {ad.ScheduledPublishTime.Value.ToLocalTime():dd.MM.yy HH:mm}" : "⚠️ Kutilmoqda");
                
            sb.AppendLine($"{displayIndex}. <b>{System.Net.WebUtility.HtmlEncode(ad.Title)}</b>");
            sb.AppendLine($"   Holati: {status}");
        }

        var keyboard = InlineKeyboards.MyAdsPagination(pageAds, page, totalPages);

        if (messageId.HasValue)
        {
            await _bot.EditMessageText(chatId, messageId.Value, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
        }
    }
}
