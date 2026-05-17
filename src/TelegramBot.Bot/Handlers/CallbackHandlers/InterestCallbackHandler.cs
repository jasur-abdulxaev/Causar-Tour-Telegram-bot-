using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Bot.States;

namespace TelegramBot.Bot.Handlers.CallbackHandlers;

/// <summary>
/// Handles the "Qiziqaman ✈️" button click — records the lead.
/// </summary>
public class InterestCallbackHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ILeadService _leadService;
    private readonly IAnalyticsService _analyticsService;

    public InterestCallbackHandler(ITelegramBotClient bot, ILeadService leadService,
        IAnalyticsService analyticsService)
    {
        _bot = bot;
        _leadService = leadService;
        _analyticsService = analyticsService;
    }

    public async Task HandleAsync(CallbackQuery cb, CancellationToken ct)
    {
        // data format: "ad_interest_<adId>"
        if (!int.TryParse(cb.Data?.Replace("ad_interest_", ""), out var adId)) return;

        await _leadService.RecordClickAsync(new CreateLeadRequest
        {
            AdvertisementId = adId,
            TelegramUserId = cb.From.Id,
            Username = cb.From.Username,
            FirstName = cb.From.FirstName,
            LastName = cb.From.LastName
        }, ct);
    }
}
