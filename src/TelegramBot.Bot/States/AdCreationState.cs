using System.Text.Json;

namespace TelegramBot.Bot.States;

public enum AdCreationStep
{
    None,
    WaitingTitle,
    WaitingFullText,      // Step 2: E'lonning to'liq matni (kanalga o'zgartirilmasdan boradi)
    WaitingMedia,         // Step 3: Rasm yoki video (ixtiyoriy)
    WaitingCaptionPosition, // Step 3.5: Rasm tepasida yoki pastida matn
    WaitingPublishTime,   // Step 4: Sana / "Hozir" tugmasi
    WaitingAdminUsername, // Step 5: Telegram username (@username)
    WaitingButtonText,    // Step 6: Tugmadagi yozuv
    Confirming            // Step 7: Preview + tasdiqlash
}

public class AdCreationState
{
    public AdCreationStep Step { get; set; } = AdCreationStep.WaitingTitle;
    public string? Title { get; set; }
    public string? FullText { get; set; }      // E'lonning asosiy matni — o'zgartirilmasdan kanalga boradi
    public string? MediaFileId { get; set; }
    public string? MediaType { get; set; }     // "photo" | "video"
    public bool ShowCaptionAboveMedia { get; set; } = false;
    public DateTime? ScheduledPublishTime { get; set; }  // null = hozir
    public string? AdminUsername { get; set; } // Telegram @username
    public string? ButtonText { get; set; }    // Tugmadagi yozuv

    public static AdCreationState? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<AdCreationState>(json);
        }
        catch { return null; }
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}
