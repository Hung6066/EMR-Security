// Core/Entities/Security/FileIntegrityRecord.cs
public class FileIntegrityRecord
{
    public int Id { get; set; }
    public string FilePath { get; set; }
    public string Hash { get; set; } // SHA256
    public DateTime LastChecked { get; set; }
    public string Status { get; set; } // Baseline, Changed, New, Deleted
}