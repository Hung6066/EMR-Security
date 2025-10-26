using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers;

[ApiController]
[Route("api/ueba")]
[Authorize(Roles = "Admin,Security")]
public class UebaController : ControllerBase
{
    private readonly IUebaService _uebaService;

    public UebaController(IUebaService uebaService)
    {
        _uebaService = uebaService;
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var alerts = await _uebaService.GetAlertsAsync(from, to);
        return Ok(alerts);
    }

    [HttpGet("alerts/{id}")]
    public async Task<IActionResult> GetAlertById(long id)
    {
        var alert = await _uebaService.GetAlertByIdAsync(id);
        if (alert == null) return NotFound();
        return Ok(alert);
    }
    
    [HttpPut("alerts/{id}/status")]
    public async Task<IActionResult> UpdateAlertStatus(long id, [FromBody] UpdateStatusRequest request)
    {
        await _uebaService.UpdateAlertStatusAsync(id, request.Status);
        return NoContent();
    }
    
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var metrics = await _uebaService.GetMetricsAsync(from, to);
        return Ok(metrics);
    }

    [HttpPost("update-baselines")]
    public async Task<IActionResult> UpdateBaselines()
    {
        // Thường được gọi bởi Hangfire, nhưng để endpoint này cho admin có thể trigger thủ công
        await _uebaService.UpdateBehavioralBaselinesAsync();
        return Ok(new { message = "UEBA baseline update job started." });
    }
    
    public record UpdateStatusRequest(string Status);
}