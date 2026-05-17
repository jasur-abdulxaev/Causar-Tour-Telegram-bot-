namespace TelegramBot.Domain.Entities;

public class Hotel
{
    public int Id { get; set; }
    public int AdvertisementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public string? ExtraInfo { get; set; }
    public int SortOrder { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}
