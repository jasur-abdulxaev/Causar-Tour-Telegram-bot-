using TelegramBot.Domain.Enums;

namespace TelegramBot.Domain.Entities;

public class Lead
{
    public int Id { get; set; }
    public int AdvertisementId { get; set; }
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.Clicked;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Advertisement Advertisement { get; set; } = null!;
}
