// EMRSystem.API/Controllers/AnomalyDetectionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/anomalies")]
    [Authorize(Roles = "Admin,Security")]
    public class AnomalyDetectionController : ControllerBase
    {
        private readonly IAnomalyDetectionService _anomalyService;

        public AnomalyDetectionController(IAnomalyDetectionService anomalyService)
        {
            _anomalyService = anomalyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnomalies([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var startDate = from ?? DateTime.UtcNow.AddDays(-7);
            var endDate = to ?? DateTime.UtcNow;
            var anomalies = await _anomalyService.GetAnomaliesAsync(startDate, endDate);
            return Ok(anomalies);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAnomalies()
        {
            var anomalies = await _anomalyService.GetAnomaliesAsync(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);
            return Ok(anomalies);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // This would be implemented in the service
            // For now, returning dummy data
            var stats = new {
                totalAnomalies = 125,
                unresolvedAnomalies = 15,
                averageScore = 0.65,
                anomaliesByType = new { UserBehavior = 80, DataAccess = 45 }
            };
            return Ok(await Task.FromResult(stats));
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveAnomaly(long id)
        {
            // Implementation in service to mark anomaly as resolved
            // await _anomalyService.ResolveAnomalyAsync(id);
            return Ok();
        }

        [HttpPost("train")]
        public async Task<IActionResult> TrainModel()
        {
            await _anomalyService.TrainModelAsync();
            return Ok(new { message = "Anomaly detection model training started." });
        }
    }
}