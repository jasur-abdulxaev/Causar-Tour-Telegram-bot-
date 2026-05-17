using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Web.Models;

public enum AdStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Archived = 3
}

public class HotelDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(0, 100000)]
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

    public List<HotelDto> Hotels { get; set; } = new();
    public int TotalLeads { get; set; }
}

public class CreateAdRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MediaFileId { get; set; }
    public string? MediaType { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string TransportInfo { get; set; } = string.Empty;
    public DateTime? DepartureDate { get; set; }
    public int? NightsCount { get; set; }
    [Required]
    public string ContactInfo { get; set; } = string.Empty;
    public long CreatedByAdminId { get; set; }

    public DateTime? ScheduledPublishTime { get; set; }
    public bool IsPublished { get; set; } = false;
    public string? CustomText { get; set; }
    public string? AdminUsername { get; set; }

    public List<HotelDto> Hotels { get; set; } = new();
}

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

    public List<HotelDto>? Hotels { get; set; }
}

public class AnalyticsResponse
{
    public int AdvertisementId { get; set; }
    public string AdTitle { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public int TotalMessagesSent { get; set; }
    public double ClickThroughRate { get; set; }
    public double ConversionRate { get; set; }
}
