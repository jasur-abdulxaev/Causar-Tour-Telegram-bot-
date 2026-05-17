namespace TelegramBot.Application.DTOs.Responses;

public class AnalyticsResponse
{
    public int AdvertisementId { get; set; }
    public string AdTitle { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public int TotalMessagesSent { get; set; }
    public double ClickThroughRate => TotalViews == 0 ? 0 : Math.Round((double)TotalClicks / TotalViews * 100, 2);
    public double ConversionRate => TotalClicks == 0 ? 0 : Math.Round((double)TotalMessagesSent / TotalClicks * 100, 2);
}
