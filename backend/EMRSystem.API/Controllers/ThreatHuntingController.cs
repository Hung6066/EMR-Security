// EMRSystem.API/Controllers/ThreatHuntingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/threat-hunting")]
    [Authorize(Roles = "Admin,SecurityAnalyst")]
    public class ThreatHuntingController : ControllerBase
    {
        private readonly IThreatHuntingService _huntingService;

        public ThreatHuntingController(IThreatHuntingService huntingService)
        {
            _huntingService = huntingService;
        }

        [HttpPost("hunt")]
        public async Task<IActionResult> HuntSuspiciousActivities([FromBody] HuntingCriteria criteria)
        {
            var activities = await _huntingService.HuntSuspiciousActivitiesAsync(criteria);
            return Ok(activities);
        }

        [HttpGet("queries")]
        public async Task<IActionResult> GetQueries()
        {
            var queries = await _huntingService.GetQueriesAsync();
            return Ok(queries);
        }

        [HttpPost("queries")]
        public async Task<IActionResult> CreateQuery([FromBody] CreateThreatQueryDto dto)
        {
            var query = await _huntingService.CreateQueryAsync(dto);
            return CreatedAtAction(nameof(GetQueryById), new { id = query.Id }, query);
        }
        
        [HttpGet("queries/{id}")]
        public async Task<IActionResult> GetQueryById(int id)
        {
            // Cần thêm phương thức GetQueryByIdAsync trong service
            // var query = await _huntingService.GetQueryByIdAsync(id);
            // if (query == null) return NotFound();
            // return Ok(query);
            return Ok(); // Placeholder
        }

        [HttpPost("execute/{queryId}")]
        public async Task<IActionResult> ExecuteQuery(int queryId)
        {
            var result = await _huntingService.ExecuteQueryAsync(queryId);
            return Ok(result);
        }

        [HttpGet("results/{queryId}")]
        public async Task<IActionResult> GetResults(int queryId)
        {
            var results = await _huntingService.GetResultsAsync(queryId);
            return Ok(results);
        }
        
        [HttpGet("indicators")]
        public async Task<IActionResult> GetIndicators()
        {
            var indicators = await _huntingService.GetIndicatorsAsync();
            return Ok(indicators);
        }

        [HttpPost("indicators")]
        public async Task<IActionResult> AddIndicator([FromBody] AddThreatIndicatorDto dto)
        {
            var indicator = await _huntingService.AddIndicatorAsync(dto);
            return Ok(indicator);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var startDate = from ?? DateTime.UtcNow.AddDays(-30);
            var endDate = to ?? DateTime.UtcNow;
            var summary = await _huntingService.GetSummaryAsync(startDate, endDate);
            return Ok(summary);
        }
    }
}