using TelegramBot.Domain.Entities;

namespace TelegramBot.Application.Interfaces.Repositories;

public interface IAdvertisementRepository
{
    Task<Advertisement?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Advertisement>> GetAllActiveAsync(CancellationToken ct = default);
    Task<List<Advertisement>> GetByAdminIdAsync(long adminId, CancellationToken ct = default);
    Task<Advertisement> CreateAsync(Advertisement ad, CancellationToken ct = default);
    Task UpdateAsync(Advertisement ad, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<Advertisement>> GetAdsToPublishAsync(DateTime upTo, CancellationToken ct = default);
}
