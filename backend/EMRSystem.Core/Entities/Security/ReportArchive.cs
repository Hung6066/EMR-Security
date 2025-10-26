public class ReportArchive
{
    public long Id { get; set; }
    public string Type { get; set; } // Security, Usage, Clinical
    public string Title { get; set; }
    public string Format { get; set; } // PDF, CSV
    public string FilePath { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? ParametersJson { get; set; }
    public int? CreatedBy { get; set; }
}