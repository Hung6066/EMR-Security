public class ReportGeneratorService : IReportGeneratorService
{
    private readonly ApplicationDbContext _ctx; private readonly IWebHostEnvironment _env;
    public ReportGeneratorService(ApplicationDbContext ctx, IWebHostEnvironment env) { _ctx = ctx; _env = env; }

    public Task<ReportArchive> GenerateSecurityReportAsync(DateTime from, DateTime to, string format)
    {
        // ví dụ: tổng hợp loginAttempts, anomalies, sessions, threats...
        var dir = Path.Combine(_env.ContentRootPath, "App_Data", "reports");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"security_{DateTime.UtcNow:yyyyMMddHHmm}.{(format=="PDF"?"pdf":"csv")}");
        // Nếu CSV: dùng StringBuilder + File.WriteAllText
        // Nếu PDF: dùng QuestPDF tạo như bạn đã triển khai trước
        // Lưu DB:
        var archive = new ReportArchive { Type="Security", Title="Security Report", Format=format, FilePath=file, GeneratedAt=DateTime.UtcNow };
        _ctx.ReportArchives.Add(archive);
        _ctx.SaveChanges();
        return Task.FromResult(archive);
    }

    public Task<ReportArchive> GenerateUsageReportAsync(DateTime from, DateTime to, string format) => Generate("Usage", from, to, format);
    public Task<ReportArchive> GenerateClinicalReportAsync(DateTime from, DateTime to, string format) => Generate("Clinical", from, to, format);
    private Task<ReportArchive> Generate(string type, DateTime from, DateTime to, string format) { /* tương tự */ return GenerateSecurityReportAsync(from,to,format); }
}