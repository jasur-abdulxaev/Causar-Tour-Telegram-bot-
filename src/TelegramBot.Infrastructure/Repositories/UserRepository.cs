using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Domain.Entities;
using TelegramBot.Infrastructure.Data;

namespace TelegramBot.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<BotUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default) =>
        _db.BotUsers.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);

    public async Task<BotUser> UpsertAsync(BotUser user, CancellationToken ct = default)
    {
        var existing = await _db.BotUsers.FindAsync(new object[] { user.TelegramId }, ct);
        if (existing == null)
        {
            _db.BotUsers.Add(user);
        }
        else
        {
            existing.Username = user.Username;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.LastSeen = DateTime.UtcNow;
            if (user.ConversationState != null)
                existing.ConversationState = user.ConversationState;
        }
        await _db.SaveChangesAsync(ct);
        return existing ?? user;
    }

    public Task<List<BotUser>> GetAllAdminsAsync(CancellationToken ct = default) =>
        _db.BotUsers.Where(u => u.IsAdmin).ToListAsync(ct);

    public Task<List<BotUser>> GetUsersForBroadcastAsync(int minDaysSinceLastBroadcast, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-minDaysSinceLastBroadcast);
        return _db.BotUsers
            .Where(u => !u.IsAdmin
                && (u.LastBroadcastSentAt == null || u.LastBroadcastSentAt <= cutoff))
            .ToListAsync(ct);
    }

    public async Task UpdateBroadcastSentAtAsync(IEnumerable<long> telegramIds, DateTime sentAt, CancellationToken ct = default)
    {
        var ids = telegramIds.ToList();
        await _db.BotUsers
            .Where(u => ids.Contains(u.TelegramId))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastBroadcastSentAt, sentAt), ct);
    }
}
