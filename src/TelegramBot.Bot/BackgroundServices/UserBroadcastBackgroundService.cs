using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Bot.Keyboards;

namespace TelegramBot.Bot.BackgroundServices;

/// <summary>
/// Haftada 2 marta (dushanba va payshanba, 09:00 UTC = 14:00 Toshkent) barcha
/// oddiy userlarga eng so'nggi faol e'lonni yuboradi.
/// Bu orqali bot passiv userlarni ham "isitib" turadi va lid jalb qiladi.
/// </summary>
public class UserBroadcastBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<BotSettings> _settings;
    private readonly ILogger<UserBroadcastBackgroundService> _logger;

    // Tekshiruv har 10 daqiqada — yuborish faqat jadvalga mos kunda va soatda
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

    public UserBroadcastBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<BotSettings> settings,
        ILogger<UserBroadcastBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserBroadcastBackgroundService ishga tushdi.");

        // Avval keyingi broadcast vaqtiga qadar kutamiz (start bo'yicha)
        await WaitUntilNextCheckAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _settings.Value;
            var nowUtc = DateTime.UtcNow;

            // Bugungi kun broadcast kunlaridan birimi?
            var todayName = nowUtc.DayOfWeek.ToString(); // e.g. "Monday"
            var isBroadcastDay = settings.BroadcastDays
                .Any(d => string.Equals(d, todayName, StringComparison.OrdinalIgnoreCase));

            // Broadcast soati (UTC) mos kelishini tekshiramiz
            var isBroadcastHour = nowUtc.Hour == settings.BroadcastHourUtc;

            if (isBroadcastDay && isBroadcastHour)
            {
                _logger.LogInformation(
                    "Broadcast vaqti keldi: {Day} {Hour}:xx UTC. Yuborishni boshlaymiz...",
                    todayName, nowUtc.Hour);

                await RunBroadcastAsync(settings, stoppingToken);
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task RunBroadcastAsync(BotSettings settings, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var adRepo   = scope.ServiceProvider.GetRequiredService<IAdvertisementRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            // 1. Eng so'nggi nashr qilingan aktiv e'lonni topamiz
            var publishedAds = await adRepo.GetAllActiveAsync(ct);
            var ad = publishedAds
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (ad == null)
            {
                _logger.LogWarning("Broadcast: nashr qilingan aktiv e'lon topilmadi. O'tkazib yuborildi.");
                return;
            }

            // 2. Layoqatli userlarni olamiz (adminsiz, cooldown o'tganlar)
            var users = await userRepo.GetUsersForBroadcastAsync(
                settings.BroadcastMinIntervalDays, ct);

            if (users.Count == 0)
            {
                _logger.LogInformation("Broadcast: hamma user yaqinda xabar olgan. O'tkazib yuborildi.");
                return;
            }

            _logger.LogInformation("Broadcast: {Count} ta userga e'lon #{AdId} yuboriladi.", users.Count, ad.Id);

            // 3. Tugma yasaymiz
            var targetAdmin = !string.IsNullOrWhiteSpace(ad.AdminUsername)
                ? ad.AdminUsername
                : settings.AdminUsername;
            var keyboard = InlineKeyboards.BuildInterestButton(
                ad.Id, ad.Title, targetAdmin, ad.ButtonText);

            var text = string.IsNullOrWhiteSpace(ad.CustomText) ? ad.Title : ad.CustomText;

            // 4. Har bir userga yuboramiz (rate-limit: 30 msg/sek)
            var successIds = new List<long>();
            int sent = 0, failed = 0;

            foreach (var user in users)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    if (!string.IsNullOrEmpty(ad.MediaFileId))
                    {
                        if (ad.MediaType == "video")
                            await botClient.SendVideo(user.TelegramId, ad.MediaFileId,
                                caption: text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                                showCaptionAboveMedia: ad.ShowCaptionAboveMedia,
                                replyMarkup: keyboard, cancellationToken: ct);
                        else
                            await botClient.SendPhoto(user.TelegramId, ad.MediaFileId,
                                caption: text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                                showCaptionAboveMedia: ad.ShowCaptionAboveMedia,
                                replyMarkup: keyboard, cancellationToken: ct);
                    }
                    else
                    {
                        await botClient.SendMessage(user.TelegramId, text,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            replyMarkup: keyboard, cancellationToken: ct);
                    }

                    successIds.Add(user.TelegramId);
                    sent++;

                    // Telegram rate-limit: 30 msg/sek — har 30 xabarda 1 sek kutamiz
                    if (sent % 25 == 0)
                        await Task.Delay(1000, ct);
                }
                catch (ApiRequestException apiEx) when (apiEx.ErrorCode is 403 or 400)
                {
                    // User botni bloklagan yoki chat topilmagan — skip
                    _logger.LogDebug("User {UserId} botni bloklagan yoki mavjud emas. Skip.", user.TelegramId);
                    failed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "User {UserId} ga xabar yuborishda xato.", user.TelegramId);
                    failed++;
                }
            }

            // 5. Muvaffaqiyatli yuborilganlarga timestamp yangilaymiz
            if (successIds.Count > 0)
                await userRepo.UpdateBroadcastSentAtAsync(successIds, DateTime.UtcNow, ct);

            _logger.LogInformation(
                "Broadcast yakunlandi: {Sent} ta yuborildi, {Failed} ta xato. E'lon: #{AdId}",
                sent, failed, ad.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Broadcast jarayonida kutilmagan xato yuz berdi.");
        }
    }

    /// <summary>
    /// Servis ishga tushganida bir soniyalik kechikish bilan boshlanadi
    /// (boshqa servislar tayyor bo'lishi uchun)
    /// </summary>
    private static async Task WaitUntilNextCheckAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), ct);
    }
}
