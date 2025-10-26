public interface ICodeAttestationService
{
    Task<AttestationResult> VerifySelfAsync();
    Task<List<LibraryVulnerability>> VerifyDependenciesAsync();
}

public class AttestationResult
{
    public bool IsValid { get; set; }
    public List<string> MismatchedFiles { get; set; }
    public DateTime VerificationTime { get; set; }
}

public class LibraryVulnerability
{
    public string PackageName { get; set; }
    public string Version { get; set; }
    public string CveId { get; set; }
    public string Severity { get; set; }
}
