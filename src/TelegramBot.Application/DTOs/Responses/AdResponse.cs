using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.DTOs.Responses;

public class HotelResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public string? ExtraInfo { get; set; }
}

public class AdResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaFileId { get; set; }
    public string? MediaType { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string TransportInfo { get; set; } = string.Empty;
    public DateTime? DepartureDate { get; set; }
    public int? NightsCount { get; set; }
    public string ContactInfo { get; set; } = string.Empty;
    public AdStatus Status { get; set; }
    public long CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ScheduledPublishTime { get; set; }
    public bool IsPublished { get; set; }
    public string? CustomText { get; set; }
    public string? AdminUsername { get; set; }
    public string? ButtonText { get; set; }
    public bool ShowCaptionAboveMedia { get; set; }

    public List<HotelResponse> Hotels { get; set; } = new();
    public int TotalLeads { get; set; }
}
