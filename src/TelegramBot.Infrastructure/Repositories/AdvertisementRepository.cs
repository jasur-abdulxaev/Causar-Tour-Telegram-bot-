using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Domain.Entities;
using TelegramBot.Infrastructure.Data;

namespace TelegramBot.Infrastructure.Repositories;

public class AdvertisementRepository : IAdvertisementRepository
{
    private readonly AppDbContext _db;

    public AdvertisementRepository(AppDbContext db) => _db = db;

    public Task<Advertisement?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Advertisements
            .Include(a => a.Hotels.OrderBy(h => h.SortOrder))
            .Include(a => a.Leads)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Advertisement>> GetAllActiveAsync(CancellationToken ct = default) =>
        _db.Advertisements
            .Where(a => a.Status == Domain.Enums.AdStatus.Active)
            .Include(a => a.Hotels.OrderBy(h => h.SortOrder))
            .Include(a => a.Leads)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public Task<List<Advertisement>> GetByAdminIdAsync(long adminId, CancellationToken ct = default) =>
        _db.Advertisements
            .Where(a => a.CreatedByAdminId == adminId)
            .Include(a => a.Hotels.OrderBy(h => h.SortOrder))
            .Include(a => a.Leads)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<Advertisement> CreateAsync(Advertisement ad, CancellationToken ct = default)
    {
        _db.Advertisements.Add(ad);
        await _db.SaveChangesAsync(ct);
        return ad;
    }

    public async Task UpdateAsync(Advertisement ad, CancellationToken ct = default)
    {
        _db.Advertisements.Update(ad);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var ad = await _db.Advertisements.FindAsync(new object[] { id }, ct);
        if (ad != null)
        {
            _db.Advertisements.Remove(ad);
            await _db.SaveChangesAsync(ct);
        }
    }

    public Task<List<Advertisement>> GetAdsToPublishAsync(DateTime upTo, CancellationToken ct = default) =>
        _db.Advertisements
            .Where(a => !a.IsPublished && a.ScheduledPublishTime != null && a.ScheduledPublishTime <= upTo && a.Status == Domain.Enums.AdStatus.Active)
            .Include(a => a.Hotels.OrderBy(h => h.SortOrder))
            .ToListAsync(ct);
}
