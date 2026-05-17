using Microsoft.AspNetCore.Mvc;
using TelegramBot.Application.DTOs.Requests;
using TelegramBot.Application.Interfaces.Services;

namespace TelegramBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _service;
    public AdvertisementsController(IAdvertisementService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var ads = await _service.GetAllActiveAsync(ct);
        return Ok(ads);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var ad = await _service.GetByIdAsync(id, ct);
        return Ok(ad);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdRequest request, CancellationToken ct)
    {
        var ad = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = ad.Id }, ad);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAdRequest request, CancellationToken ct)
    {
        var ad = await _service.UpdateAsync(id, request, ct);
        return Ok(ad);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("admin/{adminId:long}")]
    public async Task<IActionResult> GetByAdmin(long adminId, CancellationToken ct)
    {
        var ads = await _service.GetByAdminAsync(adminId, ct);
        return Ok(ads);
    }
}
