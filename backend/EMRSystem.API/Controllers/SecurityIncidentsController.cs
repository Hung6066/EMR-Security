// EMRSystem.API/Controllers/SecurityIncidentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/security-incidents")]
    [Authorize(Roles = "Admin,Security")] // Chỉ Admin và đội Security được truy cập
    public class SecurityIncidentsController : ControllerBase
    {
        private readonly ISecurityIncidentService _incidentService;

        public SecurityIncidentsController(ISecurityIncidentService incidentService)
        {
            _incidentService = incidentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetIncidents([FromQuery] string status = "active")
        {
            var incidents = status.ToLower() == "active"
                ? await _incidentService.GetActiveIncidentsAsync()
                : await _incidentService.GetAllIncidentsAsync(); // Giả sử có phương thức này

            return Ok(incidents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncidentById(long id)
        {
            var incident = await _incidentService.GetIncidentByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            return Ok(incident);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentDto dto)
        {
            var incident = await _incidentService.CreateIncidentAsync(dto);
            return CreatedAtAction(nameof(GetIncidentById), new { id = incident.Id }, incident);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            await _incidentService.UpdateIncidentStatusAsync(id, request.Status, request.Notes);
            return NoContent();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignIncident(long id, [FromBody] AssignIncidentRequest request)
        {
            await _incidentService.AssignIncidentAsync(id, request.UserId);
            return NoContent();
        }

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(long id, [FromBody] AddCommentRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _incidentService.AddCommentAsync(id, userId, request.Comment);
            return Ok();
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var startDate = from ?? DateTime.UtcNow.AddDays(-30);
            var endDate = to ?? DateTime.UtcNow;
            var metrics = await _incidentService.GetIncidentMetricsAsync(startDate, endDate);
            return Ok(metrics);
        }
        
        // --- Request DTOs ---
        public record UpdateStatusRequest(string Status, string? Notes);
        public record AssignIncidentRequest(int UserId);
        public record AddCommentRequest(string Comment);
    }
}