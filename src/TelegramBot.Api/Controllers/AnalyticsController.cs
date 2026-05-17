using Microsoft.AspNetCore.Mvc;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var stats = await _analyticsService.GetAllSummaryAsync(ct);
        return Ok(stats);
    }

    [HttpGet("{adId:int}")]
    public async Task<IActionResult> GetForAd(int adId, CancellationToken ct)
    {
        var stats = await _analyticsService.GetForAdAsync(adId, ct);
        return Ok(stats);
    }

    [HttpPost("{adId:int}/event")]
    public async Task<IActionResult> RecordEvent(int adId, [FromBody] RecordEventRequest request, CancellationToken ct)
    {
        await _analyticsService.RecordEventAsync(adId, request.UserId, request.EventType, ct);
        return Ok();
    }
}

public record RecordEventRequest(long UserId, string EventType);
