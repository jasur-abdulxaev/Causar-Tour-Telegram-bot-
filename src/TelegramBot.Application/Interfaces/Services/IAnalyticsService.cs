using TelegramBot.Application.DTOs.Responses;

namespace TelegramBot.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task RecordEventAsync(int adId, long userId, string eventType, CancellationToken ct = default);
    Task<AnalyticsResponse> GetForAdAsync(int adId, CancellationToken ct = default);
    Task<List<AnalyticsResponse>> GetAllSummaryAsync(CancellationToken ct = default);
}
