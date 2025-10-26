[ApiController]
[Route("api/classification")]
[Authorize(Roles="Admin,Security")]
public class ClassificationController : ControllerBase
{
    private readonly IDataClassificationService _svc;
    public ClassificationController(IDataClassificationService svc) { _svc = svc; }

    [HttpGet("labels")] public Task<List<ClassificationLabel>> Labels() => _svc.GetLabelsAsync();

    [HttpPost("labels")] public Task<ClassificationLabel> Upsert([FromBody] ClassificationLabel l) => _svc.UpsertLabelAsync(l);

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(string type, long id, int labelId, string? reason)
    { await _svc.AssignLabelAsync(type, id, labelId, reason); return Ok(); }

    [HttpGet("tags")] public Task<List<EntityTag>> Tags(string type, long id) => _svc.GetTagsAsync(type, id);

    [HttpPost("tags")] public async Task<IActionResult> AddTag(string type, long id, string tag)
    { await _svc.AddTagAsync(type, id, tag); return Ok(); }
}