using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;

namespace TelegramBot.Application.Interfaces.Services;

public interface IAdvertisementService
{
    Task<AdResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<AdResponse>> GetAllActiveAsync(CancellationToken ct = default);
    Task<List<AdResponse>> GetByAdminAsync(long adminId, CancellationToken ct = default);
    Task<AdResponse> CreateAsync(CreateAdRequest request, CancellationToken ct = default);
    Task<AdResponse> UpdateAsync(int id, UpdateAdRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
