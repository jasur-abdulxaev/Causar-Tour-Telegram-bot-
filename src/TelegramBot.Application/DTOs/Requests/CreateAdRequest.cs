namespace TelegramBot.Application.DTOs.Requests;

public class HotelDto
{
    public string Name { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public string? ExtraInfo { get; set; }
}

public class CreateAdRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaFileId { get; set; }
    public string? MediaType { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string TransportInfo { get; set; } = string.Empty;
    public DateTime? DepartureDate { get; set; }
    public int? NightsCount { get; set; }
    public string ContactInfo { get; set; } = string.Empty;
    public long CreatedByAdminId { get; set; }
    
    public DateTime? ScheduledPublishTime { get; set; }
    public bool IsPublished { get; set; } = false;
    public string? CustomText { get; set; }
    public string? AdminUsername { get; set; }
    public string? ButtonText { get; set; }
    public bool ShowCaptionAboveMedia { get; set; }

    public List<HotelDto> Hotels { get; set; } = new();
}
