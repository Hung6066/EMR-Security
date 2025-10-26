// Core/Entities/Security/DlpRule.cs
public class DlpRule
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Pattern { get; set; } // Regex
    public string DataType { get; set; } // PII, PHI, Financial
    public int Severity { get; set; } // 1-5
    public string Action { get; set; } // Block, Redact, Alert
    public bool IsActive { get; set; }
}

public class DlpIncident
{
    public long Id { get; set; }
    public int RuleId { get; set; }
    public int UserId { get; set; }
    public string Channel { get; set; } // API_Response, File_Export, Clipboard
    public string Context { get; set; } // e.g., API endpoint path
    public string MatchedContent { get; set; } // Phần dữ liệu bị lộ
    public DateTime DetectedAt { get; set; }
    public string ActionTaken { get; set; } // Blocked, Redacted, Alerted
    public DlpRule Rule { get; set; }
    public ApplicationUser User { get; set; }
}