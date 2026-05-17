using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Application.Interfaces.Services;

public interface ILeadService
{
    Task<LeadResponse> RecordClickAsync(CreateLeadRequest request, CancellationToken ct = default);
    Task<LeadResponse> UpdateStatusAsync(int leadId, LeadStatus status, CancellationToken ct = default);
    Task<List<LeadResponse>> GetByAdIdAsync(int adId, CancellationToken ct = default);
}
