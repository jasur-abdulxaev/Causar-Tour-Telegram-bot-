using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace TelegramBot.Bot.Services;

/// <summary>
/// Bot ishga tushganda Telegram serverida bot description va
/// short description ni avtomatik o'rnatadi.
/// Bu @BotFather orqali ham qilinishi mumkin, lekin kod orqali
/// production deployment'da avtomatik ishlaydi.
/// </summary>
public class BotSetupService
{
    private readonly ITelegramBotClient _bot;
    private readonly BotSettings _settings;
    private readonly ILogger<BotSetupService> _logger;

    public BotSetupService(
        ITelegramBotClient bot,
        IOptions<BotSettings> settings,
        ILogger<BotSetupService> logger)
    {
        _bot = bot;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Bot ishga tushganda bir marta chaqiriladi.
    /// </summary>
    public async Task SetupAsync(CancellationToken ct = default)
    {
        try
        {
            var channelLink = string.IsNullOrWhiteSpace(_settings.ChannelUsername)
                ? "https://t.me/causartour"
                : $"https://t.me/{_settings.ChannelUsername.TrimStart('@')}";

            // Short description (profil ostida ko'rinadi — max 120 belgi)
            var shortDesc =
                "✈️ Causar Tour — Sizning ishonchli sayohat hamkoringiz! " +
                $"Eng yaxshi turistik takliflar: {channelLink}";

            // Full description (/start bosganda ko'rinadi — max 512 belgi)
            var fullDesc =
                "🌍 Causar Tour — professional sayohat agentligi.\n\n" +
                "📦 Xizmatlarimiz:\n" +
                "• Turistik paketlar (Turkiya, Misr, Dubai va boshqalar)\n" +
                "• Aviachiptalar\n" +
                "• Viza va hujjatlarni rasmiylashtirish\n" +
                "• Hotel bronlash\n\n" +
                $"📢 Asosiy kanalimiz: {channelLink}\n\n" +
                "Bizning kanalga obuna bo'ling — haftasiga 2 marta " +
                "eng yaxshi sayohat takliflari, chegirmalar va maxsus aksiyalar!";

            await _bot.SetMyDescription(fullDesc, languageCode: "uz", cancellationToken: ct);
            await _bot.SetMyShortDescription(shortDesc, languageCode: "uz", cancellationToken: ct);

            // Buyruqlar ro'yxatini yangilaymiz (faqat user uchun)
            await _bot.SetMyCommands(
                new[]
                {
                    new Telegram.Bot.Types.BotCommand { Command = "start", Description = "Botni ishga tushirish" }
                },
                scope: new Telegram.Bot.Types.BotCommandScopeAllPrivateChats(),
                cancellationToken: ct);

            _logger.LogInformation("Bot description va buyruqlar muvaffaqiyatli o'rnatildi.");
        }
        catch (Exception ex)
        {
            // Description o'rnatish kritik emas — xato bo'lsa davom etamiz
            _logger.LogWarning(ex, "Bot description o'rnatishda xato. Bot ishlashda davom etadi.");
        }
    }
}
