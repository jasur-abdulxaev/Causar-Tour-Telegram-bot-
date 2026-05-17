using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Keyboards;

namespace TelegramBot.Bot.Services;

public class ChannelPublisherService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IAdvertisementService _adService;
    private readonly BotSettings _settings;
    private readonly ILogger<ChannelPublisherService> _logger;

    public ChannelPublisherService(
        ITelegramBotClient botClient,
        IAdvertisementService adService,
        IOptions<BotSettings> settings,
        ILogger<ChannelPublisherService> logger)
    {
        _botClient = botClient;
        _adService = adService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> PublishAdAsync(int adId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.ChannelId))
            {
                _logger.LogWarning("ChannelId is not configured. Cannot publish ad {AdId}.", adId);
                return false;
            }

            var ad = await _adService.GetByIdAsync(adId, ct);
            if (ad == null || ad.IsPublished) return false;

            // ── E'lon matni ─────────────────────────────────────────────────
            // CustomText — admindan kelgan, o'zgartirilmagan matn.
            // ParseMode = Html bo'lgani uchun agar matnda HTML teglari bo'lsa ishlaydi.
            // Agar plain text yuborilgan bo'lsa — Telegram uni xuddi shu ko'rinishda chiqaradi.
            var text = string.IsNullOrWhiteSpace(ad.CustomText)
                ? ad.Title  // Fallback — faqat sarlavha
                : ad.CustomText;

            // ── Tugma ─────────────────────────────────────────────────────
            var targetAdmin = !string.IsNullOrWhiteSpace(ad.AdminUsername)
                ? ad.AdminUsername
                : _settings.AdminUsername;

            var keyboard = InlineKeyboards.BuildInterestButton(
                ad.Id, ad.Title, targetAdmin, ad.ButtonText);

            // ── Kanal ga yuborish ─────────────────────────────────────────
            if (!string.IsNullOrEmpty(ad.MediaFileId))
            {
                if (ad.MediaType == "video")
                {
                    await _botClient.SendVideo(_settings.ChannelId, ad.MediaFileId,
                        caption: text,
                        parseMode: ParseMode.Html,
                        showCaptionAboveMedia: ad.ShowCaptionAboveMedia,
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
                else
                {
                    await _botClient.SendPhoto(_settings.ChannelId, ad.MediaFileId,
                        caption: text,
                        parseMode: ParseMode.Html,
                        showCaptionAboveMedia: ad.ShowCaptionAboveMedia,
                        replyMarkup: keyboard,
                        cancellationToken: ct);
                }
            }
            else
            {
                await _botClient.SendMessage(_settings.ChannelId, text,
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard,
                    cancellationToken: ct);
            }

            // Published deb belgilaymiz
            await _adService.UpdateAsync(ad.Id, new UpdateAdRequest { IsPublished = true }, ct);

            _logger.LogInformation(
                "Successfully published ad {AdId} to channel {ChannelId}", adId, _settings.ChannelId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ad {AdId}", adId);
            return false;
        }
    }
}
