// EMRSystem.API/Controllers/ComplianceController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/compliance")]
    [Authorize(Roles = "Admin,Security,ComplianceOfficer")]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;

        public ComplianceController(IComplianceService complianceService)
        {
            _complianceService = complianceService;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _complianceService.GetComplianceReportsAsync();
            return Ok(reports);
        }

        [HttpGet("reports/{standard}")]
        public async Task<IActionResult> GetReportByStandard(string standard)
        {
            var report = await _complianceService.GetReportByStandardAsync(standard);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpPost("assess/{standard}")]
        public async Task<IActionResult> RunAssessment(string standard)
        {
            var report = await _complianceService.RunAssessmentAsync(standard);
            return Ok(report);
        }

        [HttpGet("export/{standard}")]
        public async Task<IActionResult> ExportReport(string standard, [FromQuery] string format = "pdf")
        {
            var fileBytes = await _complianceService.ExportReportAsync(standard, format);
            var mimeType = format.ToLower() == "pdf" ? "application/pdf" : "text/csv";
            var fileName = $"compliance-report-{standard}-{DateTime.UtcNow:yyyyMMdd}.{format.ToLower()}";
            return File(fileBytes, mimeType, fileName);
        }
    }
}