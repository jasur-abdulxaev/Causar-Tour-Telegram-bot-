using TelegramBot.Domain.Entities;

namespace TelegramBot.Application.Interfaces.Repositories;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Lead>> GetByAdIdAsync(int adId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int adId, long userId, CancellationToken ct = default);
    Task<Lead> CreateAsync(Lead lead, CancellationToken ct = default);
    Task UpdateAsync(Lead lead, CancellationToken ct = default);
    Task<int> CountByAdIdAsync(int adId, CancellationToken ct = default);
}
