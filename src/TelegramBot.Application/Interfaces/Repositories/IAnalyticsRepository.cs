using TelegramBot.Domain.Entities;

namespace TelegramBot.Application.Interfaces.Repositories;

public interface IAnalyticsRepository
{
    Task RecordAsync(AdAnalytics ev, CancellationToken ct = default);
    Task<List<AdAnalytics>> GetByAdIdAsync(int adId, CancellationToken ct = default);
    Task<Dictionary<int, int>> GetClickCountsAsync(CancellationToken ct = default);
}
