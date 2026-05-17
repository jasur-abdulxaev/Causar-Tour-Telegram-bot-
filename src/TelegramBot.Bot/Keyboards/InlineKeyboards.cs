using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Bot.Keyboards;

public static class InlineKeyboards
{
    /// <summary>
    /// Kanalga yuboriladigan e'londagi "Bog'lanish" tugmasi.
    /// User tugmani bosganda Telegramni o'zidan qolmasdan, satuv manageriga
    /// havola matni yozilgan holda lichka ochiladi.
    /// </summary>
    public static InlineKeyboardMarkup BuildInterestButton(
        int adId,
        string adTitle,
        string adminUsername,
        string? buttonText = null)
    {
        var btnLabel = buttonText ?? "Bog'lanish ✈️";

        // tg://resolve — foydalanuvchi Telegramni tark etmaydi
        var preFilledText = Uri.EscapeDataString(
            $"Salom! Men \"{adTitle}\" haqida qiziqaman. Batafsil ma'lumot bera olasizmi?");
        var url = $"tg://resolve?domain={adminUsername.TrimStart('@')}&text={preFilledText}";

        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithUrl(btnLabel, url) }
        });
    }

    /// <summary>
    /// Admin uchun preview — tugma matni + tasdiqlash/bekor qilish
    /// </summary>
    public static InlineKeyboardMarkup PreviewKeyboard(
        string adTitle,
        string adminUsername,
        string? buttonText)
    {
        var btnLabel = buttonText ?? "Bog'lanish";
        var preFilledText = Uri.EscapeDataString(
            $"Salom! Men \"{adTitle}\" haqida qiziqaman. Batafsil ma'lumot bera olasizmi?");
        var url = $"tg://resolve?domain={adminUsername.TrimStart('@')}&text={preFilledText}";

        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithUrl(btnLabel, url) },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Tasdiqlash", "confirm_ad"),
                InlineKeyboardButton.WithCallbackData("✏️ Tahrirlash", "edit_ad")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", "cancel_ad")
            }
        });
    }

    /// <summary>
    /// "Hozir yuborish" + sana kiritish uchun tugma
    /// </summary>
    public static InlineKeyboardMarkup PublishNowOrLaterKeyboard() =>
        new(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🚀 Hozir yuborish", "publish_now") }
        });

    public static InlineKeyboardMarkup SkipMediaButton() =>
        new(new[] { InlineKeyboardButton.WithCallbackData("⏭ Rasmsiz davom etish", "skip_media") });

    /// <summary>
    /// Matn rasmning qayerida chiqishini tanlash
    /// </summary>
    public static InlineKeyboardMarkup CaptionPositionKeyboard() =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⬆️ Rasm ustida (Tepada)", "caption_above"),
                InlineKeyboardButton.WithCallbackData("⬇️ Rasm ostida (Pastda)", "caption_below")
            }
        });

    public static InlineKeyboardMarkup ConfirmOrCancel() =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Tasdiqlash", "confirm_ad"),
                InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", "cancel_ad")
            }
        });

    public static InlineKeyboardMarkup AdminMainMenu() =>
        new(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("➕ Yangi e'lon", "create_ad") },
            new[] { InlineKeyboardButton.WithCallbackData("📋 Mening e'lonlarim", "my_ads") },
            new[] { InlineKeyboardButton.WithCallbackData("📊 Statistika", "stats_all") },
        });

    public static InlineKeyboardMarkup AdminAdList(int adId, string adTitle) =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📢 Kanalga yuborish", $"publish_ad_{adId}")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🗑 O'chirish", $"delete_ad_{adId}")
            }
        });

    public static InlineKeyboardMarkup DeleteConfirm(int adId) =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Ha, o'chirilsin", $"confirm_delete_{adId}"),
                InlineKeyboardButton.WithCallbackData("❌ Yo'q", "cancel")
            }
        });
    public static InlineKeyboardMarkup MyAdsPagination(System.Collections.Generic.List<TelegramBot.Application.DTOs.Responses.AdResponse> pageAds, int currentPage, int totalPages)
    {
        var buttons = new System.Collections.Generic.List<InlineKeyboardButton[]>();
        
        // 4 tugmadan 2 qator qilish (agar 8 ta bo'lsa)
        var numberRow1 = new System.Collections.Generic.List<InlineKeyboardButton>();
        var numberRow2 = new System.Collections.Generic.List<InlineKeyboardButton>();

        for (int i = 0; i < pageAds.Count; i++)
        {
            var ad = pageAds[i];
            var btn = InlineKeyboardButton.WithCallbackData($"{i + 1}", $"view_ad_{ad.Id}");
            if (i < 4) numberRow1.Add(btn);
            else numberRow2.Add(btn);
        }

        if (numberRow1.Count > 0) buttons.Add(numberRow1.ToArray());
        if (numberRow2.Count > 0) buttons.Add(numberRow2.ToArray());

        // Pagination row
        var navRow = new System.Collections.Generic.List<InlineKeyboardButton>();
        
        if (currentPage > 1)
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"my_ads_page_{currentPage - 1}"));
            
        navRow.Add(InlineKeyboardButton.WithCallbackData("❌ Yopish", "delete_msg"));
        
        if (currentPage < totalPages)
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"my_ads_page_{currentPage + 1}"));

        buttons.Add(navRow.ToArray());

        return new InlineKeyboardMarkup(buttons);
    }
}
