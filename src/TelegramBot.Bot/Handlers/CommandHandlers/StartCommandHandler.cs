using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Keyboards;

namespace TelegramBot.Bot.Handlers.CommandHandlers;

public class StartCommandHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IAdvertisementService _adService;

    public StartCommandHandler(ITelegramBotClient bot, IAdvertisementService adService)
    {
        _bot = bot;
        _adService = adService;
    }

    public async Task HandleAsync(Message msg, bool isAdmin, CancellationToken ct)
    {
        var chatId = msg.Chat.Id;

        if (isAdmin)
        {
            await _bot.SendMessage(
                chatId,
                "👋 *Admin paneliga xush kelibsiz\\!*\n\n" +
                "Quyidagi tugmalardan birini tanlang:",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                replyMarkup: InlineKeyboards.AdminMainMenu(),
                cancellationToken: ct);
        }
        else
        {
            var welcomeText = 
                "🌎 *Causar Tour* — Sizning ishonchli sayohat hamkoringiz\\!\n\n" +
                "✈️ Xizmatlarimiz:\n" +
                "• 🌐 Turistik paketlar\n" +
                "• 🛫 Aviachiptalar\n" +
                "• 📄 Viza va hujjatlarni rasmiylashtirish\n\n" +
                "Asosiy kanalimiz linki:\n" +
                "https://t\\.me/causartour";

            var channelButton = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
            {
                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithUrl("📢 Kanalimizga o'tish", "https://t.me/causartour")
            });

            await _bot.SendMessage(
                chatId,
                welcomeText,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                replyMarkup: channelButton,
                cancellationToken: ct);
        }
    }
}
