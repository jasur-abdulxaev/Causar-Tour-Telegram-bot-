using Microsoft.AspNetCore.Mvc;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.Interfaces.Services;
using TelegramBot.Domain.Enums;

namespace TelegramBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;
    public LeadsController(ILeadService leadService) => _leadService = leadService;

    [HttpPost]
    public async Task<IActionResult> RecordClick([FromBody] CreateLeadRequest request, CancellationToken ct)
    {
        var lead = await _leadService.RecordClickAsync(request, ct);
        return Ok(lead);
    }

    [HttpGet("ad/{adId:int}")]
    public async Task<IActionResult> GetByAd(int adId, CancellationToken ct)
    {
        var leads = await _leadService.GetByAdIdAsync(adId, ct);
        return Ok(leads);
    }

    [HttpPatch("{leadId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int leadId, [FromBody] LeadStatus status, CancellationToken ct)
    {
        var lead = await _leadService.UpdateStatusAsync(leadId, status, ct);
        return Ok(lead);
    }
}
