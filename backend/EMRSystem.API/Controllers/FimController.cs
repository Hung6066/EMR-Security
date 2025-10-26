[Authorize(Roles="Admin,Security")] [Route("api/security/fim")]
public class FimController : ControllerBase {
    private readonly IFimService _svc;
    public FimController(IFimService svc) { _svc = svc; }
    [HttpGet("status")] public Task<List<FileIntegrityRecord>> Get() => _svc.GetCurrentStatusAsync();
    [HttpPost("baseline")] public async Task<IActionResult> CreateBaseline() { await _svc.CreateBaselineAsync(); return Ok(); }
    [HttpPost("scan")] public Task<List<FileIntegrityRecord>> Scan() => _svc.ScanForChangesAsync();
    [HttpPost("acknowledge/{id}")] public async Task<IActionResult> Ack(int id) { await _svc.AcknowledgeChangeAsync(id); return Ok(); }
}