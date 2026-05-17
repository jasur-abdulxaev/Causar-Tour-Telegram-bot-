using TelegramBot.Domain.Entities;

namespace TelegramBot.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<BotUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<BotUser> UpsertAsync(BotUser user, CancellationToken ct = default);
    Task<List<BotUser>> GetAllAdminsAsync(CancellationToken ct = default);

    /// <summary>
    /// Broadcast olish uchun layoqatli oddiy userlarni qaytaradi.
    /// Faqat admin bo'lmaganlar va oxirgi broadcast'dan beri
    /// <paramref name="minDaysSinceLastBroadcast"/> kun o'tganlar tanlanadi.
    /// </summary>
    Task<List<BotUser>> GetUsersForBroadcastAsync(int minDaysSinceLastBroadcast, CancellationToken ct = default);

    /// <summary>
    /// Userlarga broadcast yuborilgandan keyin LastBroadcastSentAt ni yangilaydi.
    /// </summary>
    Task UpdateBroadcastSentAtAsync(IEnumerable<long> telegramIds, DateTime sentAt, CancellationToken ct = default);
}
