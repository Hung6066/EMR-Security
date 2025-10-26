// ReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var stats = await _service.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("patients")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<PatientStatisticsDto>> GetPatientStatistics()
        {
            var stats = await _service.GetPatientStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("appointments")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<AppointmentStatisticsDto>> GetAppointmentStatistics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            var stats = await _service.GetAppointmentStatisticsAsync(start, end);
            return Ok(stats);
        }

        [HttpGet("medical-records")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<MedicalRecordStatisticsDto>> GetMedicalRecordStatistics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            var stats = await _service.GetMedicalRecordStatisticsAsync(start, end);
            return Ok(stats);
        }
    }
}