namespace TelegramBot.Domain.Entities;

public class BotUser
{
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsAdmin { get; set; }
    public string PreferredLanguage { get; set; } = "ru";  // "ru" | "uz"
    public string? ConversationState { get; set; }          // JSON state for multi-step flows
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foydalanuvchiga eng oxirgi broadcast yuborilgan vaqt.
    /// Haftada 2 marta chegarani nazorat qilish uchun ishlatiladi.
    /// </summary>
    public DateTime? LastBroadcastSentAt { get; set; }
}
