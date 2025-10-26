[Authorize(Roles="Admin,Security")] [Route("api/security/dlp")]
public class DlpController : ControllerBase {
    private readonly IDlpService _svc;
    public DlpController(IDlpService svc) { _svc = svc; }
    [HttpGet("rules")] public Task<List<DlpRule>> GetRules() => _svc.GetRulesAsync();
    [HttpPost("rules")] public Task<DlpRule> UpsertRule([FromBody] DlpRule r) => _svc.UpsertRuleAsync(r);
    [HttpGet("incidents")] public Task<List<DlpIncident>> GetIncidents(DateTime f, DateTime t) => _svc.GetIncidentsAsync(f,t);
}