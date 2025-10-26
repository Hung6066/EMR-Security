public class SessionRecording
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? Meta { get; set; } // JSON page paths
    public string StoragePath { get; set; } // path .jsonl or gz
    public long SizeBytes { get; set; }
}