using TelegramBot.Domain.Enums;

namespace TelegramBot.Domain.Entities;

public class Advertisement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaFileId { get; set; }          // Telegram file_id
    public string? MediaType { get; set; }            // "photo" | "video"
    public string Direction { get; set; } = string.Empty;   // e.g. "Toshkent — Antaliya"
    public string TransportInfo { get; set; } = string.Empty;
    public DateTime? DepartureDate { get; set; }
    public int? NightsCount { get; set; }
    public string ContactInfo { get; set; } = string.Empty;
    public AdStatus Status { get; set; } = AdStatus.Active;
    public long CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Feature: Scheduled Posts
    public DateTime? ScheduledPublishTime { get; set; }
    public bool IsPublished { get; set; } = false;

    // Feature: Caption layout
    public bool ShowCaptionAboveMedia { get; set; } = false;

    // Feature: Custom template & Specific admin linking
    public string? CustomText { get; set; }
    public string? AdminUsername { get; set; }
    public string? ButtonText { get; set; }

    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<AdAnalytics> Analytics { get; set; } = new List<AdAnalytics>();
}
