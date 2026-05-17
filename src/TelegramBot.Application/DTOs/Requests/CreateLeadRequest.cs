namespace TelegramBot.Application.DTOs.Requests;

public class CreateLeadRequest
{
    public int AdvertisementId { get; set; }
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
