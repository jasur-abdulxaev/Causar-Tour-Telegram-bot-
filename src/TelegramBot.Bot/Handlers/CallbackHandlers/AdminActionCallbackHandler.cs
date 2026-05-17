using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.Handlers.MessageHandlers;
using TelegramBot.Bot.Keyboards;
using TelegramBot.Bot.Services;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers.CallbackHandlers;

public class AdminActionCallbackHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IAdvertisementService _adService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IUserRepository _userRepo;
    private readonly ChannelPublisherService _publisherService;
    private readonly TextMessageHandler _textHandler;
    private readonly BotSettings _settings;

    public AdminActionCallbackHandler(
        ITelegramBotClient bot,
        IAdvertisementService adService,
        IAnalyticsService analyticsService,
        IUserRepository userRepo,
        ChannelPublisherService publisherService,
        TextMessageHandler textHandler,
        IOptions<BotSettings> settings)
    {
        _bot = bot;
        _adService = adService;
        _analyticsService = analyticsService;
        _userRepo = userRepo;
        _publisherService = publisherService;
        _textHandler = textHandler;
        _settings = settings.Value;
    }

    public async Task HandleAsync(CallbackQuery cb, CancellationToken ct)
    {
        var data = cb.Data ?? "";
        var chatId = cb.Message!.Chat.Id;
        var fromId = cb.From.Id;

        switch (data)
        {
            // ── "Hozir yuborish" tugmasi ───────────────────────────────────
            case "publish_now":
            {
                var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
                var state = AdCreationState.FromJson(user?.ConversationState);
                if (state == null) return;

                state.ScheduledPublishTime = null; // null = hozir

                // Keyingi qadam: admin username so'rash
                state.Step = AdCreationStep.WaitingAdminUsername;
                user!.ConversationState = state.ToJson();
                await _userRepo.UpsertAsync(user, ct);

                await _bot.SendMessage(chatId,
                    "🚀 Hozir yuborish tanlandi!\n\n" +
                    "<b>👤 Telegram username kiriting</b>\n\n" +
                    "Foydalanuvchi tugmani bosganda shu adminning lichkasiga o'tadi.\n" +
                    "Misol: <code>@Madina_manager</code>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                break;
            }

            // ── Media o'tkazib yuborish ────────────────────────────────────
            case "skip_media":
            {
                var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
                var state = AdCreationState.FromJson(user?.ConversationState);
                if (state == null || state.Step != AdCreationStep.WaitingMedia) return;

                state.MediaFileId = null;
                state.MediaType = null;
                state.Step = AdCreationStep.WaitingPublishTime;
                user!.ConversationState = state.ToJson();
                await _userRepo.UpsertAsync(user, ct);

                await _bot.SendMessage(chatId,
                    "<b>⏰ E'lon qachon chiqarilsin?</b>\n\n" +
                    "Format: <code>2026-05-20 15:00</code>\n" +
                    "Yoki hozir yuborish uchun quyidagi tugmani bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: InlineKeyboards.PublishNowOrLaterKeyboard(),
                    cancellationToken: ct);
                break;
            }

            // ── Caption Position ───────────────────────────────────────────
            case "caption_above":
            case "caption_below":
            {
                var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
                var state = AdCreationState.FromJson(user?.ConversationState);
                if (state == null || state.Step != AdCreationStep.WaitingCaptionPosition) return;

                state.ShowCaptionAboveMedia = (data == "caption_above");
                state.Step = AdCreationStep.WaitingPublishTime;
                user!.ConversationState = state.ToJson();
                await _userRepo.UpsertAsync(user, ct);

                try { await _bot.DeleteMessage(chatId, cb.Message!.MessageId, ct); } catch { }

                await _bot.SendMessage(chatId,
                    "<b>⏰ E'lon qachon chiqarilsin?</b>\n\n" +
                    "Format: <code>2026-05-20 15:00</code>\n" +
                    "Yoki hozir yuborish uchun quyidagi tugmani bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: InlineKeyboards.PublishNowOrLaterKeyboard(),
                    cancellationToken: ct);
                break;
            }

            // ── E'lonni tasdiqlash ─────────────────────────────────────────
            case "confirm_ad":
                await SaveAndPublishAdAsync(fromId, chatId, ct);
                break;

            // ── Tahrirlash — boshidan boshlash ─────────────────────────────
            case "edit_ad":
            {
                var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
                if (user != null)
                {
                    // Holatni qayta WaitingTitle ga qaytaramiz
                    var freshState = new AdCreationState { Step = AdCreationStep.WaitingTitle };
                    user.ConversationState = freshState.ToJson();
                    await _userRepo.UpsertAsync(user, ct);
                }
                await _bot.SendMessage(chatId,
                    "✏️ Yangi e'lon yaratish\n\nE'lon sarlavhasini kiriting:",
                    cancellationToken: ct);
                break;
            }

            // ── Bekor qilish ───────────────────────────────────────────────
            case "cancel_ad":
            case "cancel":
            {
                var user = await _userRepo.GetByTelegramIdAsync(fromId, ct);
                if (user != null)
                {
                    user.ConversationState = null;
                    await _userRepo.UpsertAsync(user, ct);
                }
                await _bot.SendMessage(chatId,
                    "❌ E'lon yaratish bekor qilindi.",
                    replyMarkup: InlineKeyboards.AdminMainMenu(),
                    cancellationToken: ct);
                break;
            }

            case "delete_msg":
                try { await _bot.DeleteMessage(chatId, cb.Message!.MessageId, ct); } catch { }
                break;

            // ── Qo'lda kanalga yuborish, ko'rish, o'chirish ───────────────
            default:
                if (data.StartsWith("view_ad_") && int.TryParse(data.Replace("view_ad_", ""), out var viewId))
                {
                    var ad = await _adService.GetByIdAsync(viewId, ct);
                    if (ad != null)
                    {
                        var text = MessageFormatterService.FormatAd(ad);
                        var keyboard = InlineKeyboards.AdminAdList(ad.Id, ad.Title);

                        if (!string.IsNullOrEmpty(ad.MediaFileId))
                        {
                            if (ad.MediaType == "video")
                                await _bot.SendVideo(chatId, ad.MediaFileId, caption: text, parseMode: ParseMode.Html, showCaptionAboveMedia: ad.ShowCaptionAboveMedia, replyMarkup: keyboard, cancellationToken: ct);
                            else
                                await _bot.SendPhoto(chatId, ad.MediaFileId, caption: text, parseMode: ParseMode.Html, showCaptionAboveMedia: ad.ShowCaptionAboveMedia, replyMarkup: keyboard, cancellationToken: ct);
                        }
                        else
                        {
                            await _bot.SendMessage(chatId, text, parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
                        }
                    }
                }
                else if (data.StartsWith("publish_ad_") && int.TryParse(data.Replace("publish_ad_", ""), out var pubId))
                {
                    var ok = await _publisherService.PublishAdAsync(pubId, ct);
                    await _bot.SendMessage(chatId,
                        ok ? "✅ E'lon kanalga yuborildi!" : "❌ Xatolik yuz berdi.",
                        cancellationToken: ct);
                }
                else if (data == "list_ads")
                {
                    await _bot.SendMessage(chatId,
                        "Bosh menyu:",
                        replyMarkup: InlineKeyboards.AdminMainMenu(),
                        cancellationToken: ct);
                }
                break;
        }
    }

    private async Task SaveAndPublishAdAsync(long adminId, long chatId, CancellationToken ct)
    {
        var user = await _userRepo.GetByTelegramIdAsync(adminId, ct);
        var state = AdCreationState.FromJson(user?.ConversationState);
        if (state == null) return;

        var request = new CreateAdRequest
        {
            Title       = state.Title ?? "Untitled",
            Description = "",
            MediaFileId = state.MediaFileId,
            MediaType   = state.MediaType,
            Direction   = "",
            TransportInfo = "",
            DepartureDate = null,
            NightsCount   = null,
            ContactInfo   = "",
            CustomText    = state.FullText,       // To'liq matn — o'zgartirilmagan
            AdminUsername = state.AdminUsername,
            ButtonText    = state.ButtonText,
            ShowCaptionAboveMedia = state.ShowCaptionAboveMedia,
            ScheduledPublishTime = state.ScheduledPublishTime,
            CreatedByAdminId = adminId,
            Hotels = new List<HotelDto>()
        };

        var ad = await _adService.CreateAsync(request, ct);

        user!.ConversationState = null;
        await _userRepo.UpsertAsync(user, ct);

        if (!state.ScheduledPublishTime.HasValue)
        {
            // Hozir yuborish
            var ok = await _publisherService.PublishAdAsync(ad.Id, ct);
            await _bot.SendMessage(chatId,
                ok
                    ? $"✅ <b>E'lon #{ad.Id} kanalga yuborildi!</b>"
                    : $"⚠️ E'lon #{ad.Id} saqlandi, lekin kanalga yuborishda xatolik.",
                parseMode: ParseMode.Html,
                replyMarkup: InlineKeyboards.AdminMainMenu(),
                cancellationToken: ct);
        }
        else
        {
            var localTime = state.ScheduledPublishTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            await _bot.SendMessage(chatId,
                $"✅ <b>E'lon #{ad.Id} rejaga qo'shildi!</b>\n" +
                $"⏰ Yuborilish vaqti: <b>{localTime}</b>",
                parseMode: ParseMode.Html,
                replyMarkup: InlineKeyboards.AdminMainMenu(),
                cancellationToken: ct);
        }
    }
}
