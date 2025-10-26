public interface IReportGeneratorService
{
    Task<ReportArchive> GenerateSecurityReportAsync(DateTime from, DateTime to, string format);
    Task<ReportArchive> GenerateUsageReportAsync(DateTime from, DateTime to, string format);
    Task<ReportArchive> GenerateClinicalReportAsync(DateTime from, DateTime to, string format);
}