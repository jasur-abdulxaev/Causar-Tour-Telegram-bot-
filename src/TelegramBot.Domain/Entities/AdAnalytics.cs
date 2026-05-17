namespace TelegramBot.Domain.Entities;

public class AdAnalytics
{
    public int Id { get; set; }
    public int AdvertisementId { get; set; }
    public string EventType { get; set; } = string.Empty;  // "view" | "click" | "message_sent"
    public long TelegramUserId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Advertisement Advertisement { get; set; } = null!;
}
