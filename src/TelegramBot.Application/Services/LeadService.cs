using AutoMapper;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Entities;
using TelegramBot.Domain.Enums;
using TelegramBot.Domain.Exceptions;

namespace TelegramBot.Application.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepo;
    private readonly IAdvertisementRepository _adRepo;
    private readonly IAnalyticsService _analyticsService;
    private readonly IMapper _mapper;

    public LeadService(ILeadRepository leadRepo, IAdvertisementRepository adRepo,
        IAnalyticsService analyticsService, IMapper mapper)
    {
        _leadRepo = leadRepo;
        _adRepo = adRepo;
        _analyticsService = analyticsService;
        _mapper = mapper;
    }

    public async Task<LeadResponse> RecordClickAsync(CreateLeadRequest request, CancellationToken ct = default)
    {
        _ = await _adRepo.GetByIdAsync(request.AdvertisementId, ct)
            ?? throw new NotFoundException(nameof(Advertisement), request.AdvertisementId);

        // Upsert: if already exists, just return existing
        var exists = await _leadRepo.ExistsAsync(request.AdvertisementId, request.TelegramUserId, ct);
        if (!exists)
        {
            var lead = new Lead
            {
                AdvertisementId = request.AdvertisementId,
                TelegramUserId = request.TelegramUserId,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = LeadStatus.Clicked
            };
            var created = await _leadRepo.CreateAsync(lead, ct);
            await _analyticsService.RecordEventAsync(request.AdvertisementId, request.TelegramUserId, "click", ct);
            return _mapper.Map<LeadResponse>(created);
        }

        var leads = await _leadRepo.GetByAdIdAsync(request.AdvertisementId, ct);
        var existing = leads.First(l => l.TelegramUserId == request.TelegramUserId);
        return _mapper.Map<LeadResponse>(existing);
    }

    public async Task<LeadResponse> UpdateStatusAsync(int leadId, LeadStatus status, CancellationToken ct = default)
    {
        var lead = await _leadRepo.GetByIdAsync(leadId, ct)
            ?? throw new NotFoundException(nameof(Lead), leadId);
        lead.Status = status;
        if (status == LeadStatus.MessageSent)
            await _analyticsService.RecordEventAsync(lead.AdvertisementId, lead.TelegramUserId, "message_sent", ct);
        await _leadRepo.UpdateAsync(lead, ct);
        return _mapper.Map<LeadResponse>(lead);
    }

    public async Task<List<LeadResponse>> GetByAdIdAsync(int adId, CancellationToken ct = default)
    {
        var leads = await _leadRepo.GetByAdIdAsync(adId, ct);
        return _mapper.Map<List<LeadResponse>>(leads);
    }
}
