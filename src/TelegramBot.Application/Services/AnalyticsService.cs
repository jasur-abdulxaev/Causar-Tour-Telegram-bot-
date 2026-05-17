using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Entities;
using TelegramBot.Domain.Exceptions;

namespace TelegramBot.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepo;
    private readonly IAdvertisementRepository _adRepo;

    public AnalyticsService(IAnalyticsRepository analyticsRepo, IAdvertisementRepository adRepo)
    {
        _analyticsRepo = analyticsRepo;
        _adRepo = adRepo;
    }

    public async Task RecordEventAsync(int adId, long userId, string eventType, CancellationToken ct = default)
    {
        await _analyticsRepo.RecordAsync(new AdAnalytics
        {
            AdvertisementId = adId,
            TelegramUserId = userId,
            EventType = eventType,
            OccurredAt = DateTime.UtcNow
        }, ct);
    }

    public async Task<AnalyticsResponse> GetForAdAsync(int adId, CancellationToken ct = default)
    {
        var ad = await _adRepo.GetByIdAsync(adId, ct)
            ?? throw new NotFoundException(nameof(Advertisement), adId);

        var events = await _analyticsRepo.GetByAdIdAsync(adId, ct);
        return BuildResponse(ad, events);
    }

    public async Task<List<AnalyticsResponse>> GetAllSummaryAsync(CancellationToken ct = default)
    {
        var ads = await _adRepo.GetAllActiveAsync(ct);
        var result = new List<AnalyticsResponse>();
        foreach (var ad in ads)
        {
            var events = await _analyticsRepo.GetByAdIdAsync(ad.Id, ct);
            result.Add(BuildResponse(ad, events));
        }
        return result.OrderByDescending(r => r.TotalClicks).ToList();
    }

    private static AnalyticsResponse BuildResponse(Advertisement ad, List<AdAnalytics> events) => new()
    {
        AdvertisementId = ad.Id,
        AdTitle = ad.Title,
        TotalViews = events.Count(e => e.EventType == "view"),
        TotalClicks = events.Count(e => e.EventType == "click"),
        TotalMessagesSent = events.Count(e => e.EventType == "message_sent")
    };
}
