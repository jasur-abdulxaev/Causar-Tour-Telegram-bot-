using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Domain.Entities;
using TelegramBot.Infrastructure.Data;

namespace TelegramBot.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _db;
    public AnalyticsRepository(AppDbContext db) => _db = db;

    public async Task RecordAsync(AdAnalytics ev, CancellationToken ct = default)
    {
        _db.AdAnalytics.Add(ev);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<AdAnalytics>> GetByAdIdAsync(int adId, CancellationToken ct = default) =>
        _db.AdAnalytics.Where(a => a.AdvertisementId == adId).ToListAsync(ct);

    public async Task<Dictionary<int, int>> GetClickCountsAsync(CancellationToken ct = default)
    {
        return await _db.AdAnalytics
            .Where(a => a.EventType == "click")
            .GroupBy(a => a.AdvertisementId)
            .Select(g => new { AdId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AdId, x => x.Count, ct);
    }
}
