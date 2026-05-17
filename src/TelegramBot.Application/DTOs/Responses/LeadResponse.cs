using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.DTOs.Responses;

public class LeadResponse
{
    public int Id { get; set; }
    public int AdvertisementId { get; set; }
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public LeadStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
