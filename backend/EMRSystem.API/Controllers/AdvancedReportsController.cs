[ApiController]
[Route("api/advanced-reports")]
[Authorize(Roles="Admin,Doctor")]
public class AdvancedReportsController : ControllerBase
{
    private readonly IReportGeneratorService _svc; private readonly ApplicationDbContext _ctx;
    public AdvancedReportsController(IReportGeneratorService svc, ApplicationDbContext ctx) { _svc = svc; _ctx = ctx; }

    [HttpPost("export")]
    public async Task<IActionResult> Export([FromQuery] string type, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string format="PDF")
    {
        ReportArchive r = type switch
        {
            "Security" => await _svc.GenerateSecurityReportAsync(from,to,format),
            "Usage" => await _svc.GenerateUsageReportAsync(from,to,format),
            "Clinical" => await _svc.GenerateClinicalReportAsync(from,to,format),
            _ => throw new Exception("Unknown report type")
        };
        return Ok(r);
    }

    [HttpGet("archive")]
    public async Task<IActionResult> Archive() => Ok(await _ctx.ReportArchives.OrderByDescending(x=>x.GeneratedAt).Take(200).ToListAsync());

    [HttpGet("archive/{id:long}/download")]
    public async Task<IActionResult> Download(long id)
    {
        var r = await _ctx.ReportArchives.FindAsync(id);
        if (r == null || !System.IO.File.Exists(r.FilePath)) return NotFound();
        var mime = r.Format=="PDF" ? "application/pdf" : "text/csv";
        return PhysicalFile(r.FilePath, mime, Path.GetFileName(r.FilePath));
    }
}