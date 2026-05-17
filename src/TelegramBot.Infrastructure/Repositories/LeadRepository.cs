using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Domain.Entities;
using TelegramBot.Infrastructure.Data;

namespace TelegramBot.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly AppDbContext _db;
    public LeadRepository(AppDbContext db) => _db = db;

    public Task<Lead?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Leads.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<Lead>> GetByAdIdAsync(int adId, CancellationToken ct = default) =>
        _db.Leads.Where(l => l.AdvertisementId == adId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int adId, long userId, CancellationToken ct = default) =>
        _db.Leads.AnyAsync(l => l.AdvertisementId == adId && l.TelegramUserId == userId, ct);

    public async Task<Lead> CreateAsync(Lead lead, CancellationToken ct = default)
    {
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);
        return lead;
    }

    public async Task UpdateAsync(Lead lead, CancellationToken ct = default)
    {
        _db.Leads.Update(lead);
        await _db.SaveChangesAsync(ct);
    }

    public Task<int> CountByAdIdAsync(int adId, CancellationToken ct = default) =>
        _db.Leads.CountAsync(l => l.AdvertisementId == adId, ct);
}
