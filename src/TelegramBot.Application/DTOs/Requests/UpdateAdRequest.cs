using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.DTOs.Requests;

public class UpdateAdRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? MediaFileId { get; set; }
    public string? MediaType { get; set; }
    public string? Direction { get; set; }
    public string? TransportInfo { get; set; }
    public DateTime? DepartureDate { get; set; }
    public int? NightsCount { get; set; }
    public string? ContactInfo { get; set; }
    public AdStatus? Status { get; set; }
    
    public DateTime? ScheduledPublishTime { get; set; }
    public bool? IsPublished { get; set; }
    public string? CustomText { get; set; }
    public string? AdminUsername { get; set; }
    public bool? ShowCaptionAboveMedia { get; set; }

    public List<HotelDto>? Hotels { get; set; }
}
