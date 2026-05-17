using AutoMapper;
using Microsoft.Extensions.Logging;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.DTOs.Responses;
using TelegramBot.Application.Interfaces.Repositories;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Entities;
using TelegramBot.Domain.Exceptions;

namespace TelegramBot.Application.Services;

public class AdvertisementService : IAdvertisementService
{
    private readonly IAdvertisementRepository _adRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<AdvertisementService> _logger;

    public AdvertisementService(IAdvertisementRepository adRepo, IMapper mapper,
        ILogger<AdvertisementService> logger)
    {
        _adRepo = adRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AdResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var ad = await _adRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Advertisement), id);
        return _mapper.Map<AdResponse>(ad);
    }

    public async Task<List<AdResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var ads = await _adRepo.GetAllActiveAsync(ct);
        return _mapper.Map<List<AdResponse>>(ads);
    }

    public async Task<List<AdResponse>> GetByAdminAsync(long adminId, CancellationToken ct = default)
    {
        var ads = await _adRepo.GetByAdminIdAsync(adminId, ct);
        return _mapper.Map<List<AdResponse>>(ads);
    }

    public async Task<AdResponse> CreateAsync(CreateAdRequest request, CancellationToken ct = default)
    {
        var ad = _mapper.Map<Advertisement>(request);
        // Assign sort order to hotels
        for (int i = 0; i < ad.Hotels.Count; i++)
            ((List<Hotel>)ad.Hotels)[i].SortOrder = i;

        var created = await _adRepo.CreateAsync(ad, ct);
        _logger.LogInformation("Advertisement #{Id} created by admin {AdminId}", created.Id, request.CreatedByAdminId);
        return _mapper.Map<AdResponse>(created);
    }

    public async Task<AdResponse> UpdateAsync(int id, UpdateAdRequest request, CancellationToken ct = default)
    {
        var ad = await _adRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Advertisement), id);

        if (request.Title != null) ad.Title = request.Title;
        if (request.Description != null) ad.Description = request.Description;
        if (request.MediaFileId != null) ad.MediaFileId = request.MediaFileId;
        if (request.MediaType != null) ad.MediaType = request.MediaType;
        if (request.Direction != null) ad.Direction = request.Direction;
        if (request.TransportInfo != null) ad.TransportInfo = request.TransportInfo;
        if (request.DepartureDate.HasValue) ad.DepartureDate = request.DepartureDate;
        if (request.NightsCount.HasValue) ad.NightsCount = request.NightsCount;
        if (request.ContactInfo != null) ad.ContactInfo = request.ContactInfo;
        if (request.Status.HasValue) ad.Status = request.Status.Value;
        if (request.ScheduledPublishTime.HasValue) ad.ScheduledPublishTime = request.ScheduledPublishTime;
        if (request.IsPublished.HasValue) ad.IsPublished = request.IsPublished.Value;
        if (request.CustomText != null) ad.CustomText = request.CustomText;
        if (request.AdminUsername != null) ad.AdminUsername = request.AdminUsername;
        if (request.ShowCaptionAboveMedia.HasValue) ad.ShowCaptionAboveMedia = request.ShowCaptionAboveMedia.Value;
        if (request.Hotels != null)
        {
            ad.Hotels.Clear();
            var hotels = _mapper.Map<List<Hotel>>(request.Hotels);
            for (int i = 0; i < hotels.Count; i++) hotels[i].SortOrder = i;
            foreach (var h in hotels) ad.Hotels.Add(h);
        }
        ad.UpdatedAt = DateTime.UtcNow;

        await _adRepo.UpdateAsync(ad, ct);
        return _mapper.Map<AdResponse>(ad);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _ = await _adRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Advertisement), id);
        await _adRepo.DeleteAsync(id, ct);
    }
}
