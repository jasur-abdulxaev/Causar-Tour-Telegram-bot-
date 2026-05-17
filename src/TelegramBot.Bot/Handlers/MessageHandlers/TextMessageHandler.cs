using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Bot.Keyboards;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers.MessageHandlers;

/// <summary>
/// Matnli xabarlarni qayta ishlaydi. Har bir qadamda admindan
/// navbatdagi ma'lumot so'raladi.
///
/// Tartib:
///   1. WaitingTitle         → Sarlavha
///   2. WaitingFullText      → E'lonning to'liq matni (o'zgartirilmasdan kanalga boradi)
///   3. WaitingMedia         → Rasm/video (PhotoMessageHandler) yoki "O'tkazib yuborish"
///   4. WaitingPublishTime   → Sana yoki "Hozir" tugmasi
///   5. WaitingAdminUsername → Telegram @username
///   6. WaitingButtonText    → Tugmadagi yozuv
///   7. Confirming           → Preview + tasdiqlash
/// </summary>
public class TextMessageHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepo;

    public TextMessageHandler(ITelegramBotClient bot, IUserRepository userRepo)
    {
        _bot = bot;
        _userRepo = userRepo;
    }

    public async Task HandleAsync(Message msg, CancellationToken ct)
    {
        var user = await _userRepo.GetByTelegramIdAsync(msg.From!.Id, ct);
        if (user == null || string.IsNullOrEmpty(user.ConversationState)) return;

        var state = AdCreationState.FromJson(user.ConversationState);
        if (state == null) return;

        switch (state.Step)
        {
            // ── Qadam 1: Sarlavha ──────────────────────────────────────────
            case AdCreationStep.WaitingTitle:
                state.Title = msg.Text?.Trim();
                state.Step = AdCreationStep.WaitingFullText;
                await _bot.SendMessage(msg.Chat.Id,
                    "<b>📝 E'lonning to'liq matnini yuboring</b>\n\n" +
                    "Bu matn kanalga <b>o'zgartirilmasdan</b> yuboriladi.\n" +
                    "(Emoji, formatlash — hammasi saqlanadi):",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                break;

            // ── Qadam 2: To'liq matn ───────────────────────────────────────
            case AdCreationStep.WaitingFullText:
                state.FullText = msg.Text; // O'zgartirilmaydi — HTML yoki plain text
                state.Step = AdCreationStep.WaitingMedia;
                await _bot.SendMessage(msg.Chat.Id,
                    "<b>🖼 Rasm yoki video yuboring</b>\n\n" +
                    "E'longa rasm yoki video qo'shish uchun shu yerdan yuboring.\n" +
                    "Rasmsiz davom etish uchun quyidagi tugmani bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: InlineKeyboards.SkipMediaButton(),
                    cancellationToken: ct);
                break;

            // ── Qadam 4: Sana (matn formatida) ────────────────────────────
            case AdCreationStep.WaitingPublishTime:
                // "Hozir" so'zi kiritilsa ham qabul qilamiz
                var trimmed = msg.Text?.Trim() ?? "";
                if (trimmed.Equals("hozir", StringComparison.OrdinalIgnoreCase))
                {
                    state.ScheduledPublishTime = null;
                }
                else if (DateTime.TryParse(trimmed, out var scheduledTime))
                {
                    // UTC ga o'tkazamiz (server +5 ishlamasligi uchun)
                    state.ScheduledPublishTime = scheduledTime.ToUniversalTime();
                }
                else
                {
                    await _bot.SendMessage(msg.Chat.Id,
                        "❌ Vaqt formati noto'g'ri.\n\n" +
                        "Format: <code>2026-05-20 15:00</code>\n" +
                        "Yoki hozir yuborish uchun tugmani bosing:",
                        parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.PublishNowOrLaterKeyboard(),
                        cancellationToken: ct);
                    return; // state saqlanmaydi — qayta urinish uchun
                }

                state.Step = AdCreationStep.WaitingAdminUsername;
                await _bot.SendMessage(msg.Chat.Id,
                    "<b>👤 Telegram username kiriting</b>\n\n" +
                    "Foydalanuvchi tugmani bosganda shu adminning lichkasiga o'tadi.\n" +
                    "Misol: <code>@Madina_manager</code>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                break;

            // ── Qadam 5: Admin username ─────────────────────────────────────
            case AdCreationStep.WaitingAdminUsername:
                state.AdminUsername = msg.Text?.Trim().TrimStart('@');
                state.Step = AdCreationStep.WaitingButtonText;
                await _bot.SendMessage(msg.Chat.Id,
                    "<b>🔘 Tugmada nima yozilsin?</b>\n\n" +
                    "Foydalanuvchi shu tugmani bosadi va manageringizga xabar yoziladi.\n" +
                    "Misol: <code>Madina</code>, <code>Jasur</code>, <code>Bog'lanish</code>:",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                break;

            // ── Qadam 6: Tugma matni → Preview ──────────────────────────────
            case AdCreationStep.WaitingButtonText:
                state.ButtonText = msg.Text?.Trim();
                state.Step = AdCreationStep.Confirming;
                await ShowPreviewAsync(msg.Chat.Id, state, ct);
                break;
        }

        user.ConversationState = state.ToJson();
        await _userRepo.UpsertAsync(user, ct);
    }

    /// <summary>
    /// Adminga aynan kanalga yuboriladigan e'lon ko'rinishini ko'rsatadi.
    /// Pastida "Tasdiqlash / Tahrirlash / Bekor qilish" tugmalari bo'ladi.
    /// </summary>
    internal async Task ShowPreviewAsync(long chatId, AdCreationState state, CancellationToken ct)
    {
        var adminUsername = string.IsNullOrWhiteSpace(state.AdminUsername)
            ? "username_kiritilmadi"
            : state.AdminUsername;

        var timeStr = state.ScheduledPublishTime.HasValue
            ? state.ScheduledPublishTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "🚀 Hozir";

        // ── Sarlavha ma'lumoti — faqat admin uchun ──
        var header = $"<b>📋 PREVIEW — aynan kanalga shu ko'rinishda boradi</b>\n" +
                     $"📌 Sarlavha: <b>{System.Net.WebUtility.HtmlEncode(state.Title ?? "—")}</b>\n" +
                     $"⏰ Vaqt: <b>{timeStr}</b>\n" +
                     $"👤 Manager: @{System.Net.WebUtility.HtmlEncode(adminUsername)}\n" +
                     $"🔘 Tugma: <b>{System.Net.WebUtility.HtmlEncode(state.ButtonText ?? "Bog'lanish")}</b>\n\n" +
                     $"━━━━━━━━━━━━━━━━━━\n";

        // ── E'lonning o'zi (to'liq matn, o'zgartirilmagan) ──
        var adBody = state.FullText ?? "(matn yo'q)";

        // Agar matn uzun bo'lsa, caption = header + body (max 1024 belgi)
        // Aks holda alohida xabar sifatida yuboramiz
        var keyboard = InlineKeyboards.PreviewKeyboard(
            state.Title ?? "",
            adminUsername,
            state.ButtonText);

        if (!string.IsNullOrEmpty(state.MediaFileId))
        {
            // Caption = to'liq matn (rasm/video bilan birga boradi)
            var caption = header + adBody;
            if (caption.Length > 1024) caption = caption[..1021] + "...";

            if (state.MediaType == "video")
            {
                await _bot.SendVideo(chatId, state.MediaFileId,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    showCaptionAboveMedia: state.ShowCaptionAboveMedia,
                    replyMarkup: keyboard,
                    cancellationToken: ct);
            }
            else
            {
                await _bot.SendPhoto(chatId, state.MediaFileId,
                    caption: caption,
                    parseMode: ParseMode.Html,
                    showCaptionAboveMedia: state.ShowCaptionAboveMedia,
                    replyMarkup: keyboard,
                    cancellationToken: ct);
            }
        }
        else
        {
            // Rasmsiz — header + matnni bitta message sifatida yuboramiz
            await _bot.SendMessage(chatId,
                header + adBody,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
