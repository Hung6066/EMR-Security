[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionRecordingsController : ControllerBase
{
    private readonly ApplicationDbContext _ctx;
    private readonly IWebHostEnvironment _env;

    public SessionRecordingsController(ApplicationDbContext ctx, IWebHostEnvironment env)
    { _ctx = ctx; _env = env; }

    [HttpPost("start")]
    public async Task<IActionResult> Start()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var rec = new SessionRecording
        {
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            UserAgent = Request.Headers["User-Agent"].ToString(),
            StoragePath = Path.Combine(_env.ContentRootPath, "App_Data", "recordings", $"{Guid.NewGuid():N}.jsonl")
        };
        Directory.CreateDirectory(Path.GetDirectoryName(rec.StoragePath)!);
        _ctx.SessionRecordings.Add(rec);
        await _ctx.SaveChangesAsync();
        return Ok(new { sessionId = rec.Id.ToString() });
    }

    public class ChunkDto { public string sessionId { get; set; } = ""; public JsonElement events { get; set; } }

    [HttpPost("chunk")]
    public async Task<IActionResult> Chunk([FromBody] ChunkDto dto)
    {
        if (!long.TryParse(dto.sessionId, out var id)) return BadRequest();
        var rec = await _ctx.SessionRecordings.FindAsync(id);
        if (rec == null) return NotFound();

        await System.IO.File.AppendAllTextAsync(rec.StoragePath, dto.events.ToString() + "\n");
        var fi = new FileInfo(rec.StoragePath);
        rec.SizeBytes = fi.Exists ? fi.Length : 0;
        await _ctx.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("stop")]
    public async Task<IActionResult> Stop([FromBody] dynamic body)
    {
        long id = long.Parse((string)body.sessionId);
        var rec = await _ctx.SessionRecordings.FindAsync(id);
        if (rec == null) return NotFound();
        rec.EndedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Authorize(Roles="Admin,Security")]
    public async Task<IActionResult> List() =>
        Ok(await _ctx.SessionRecordings.OrderByDescending(x => x.StartedAt).Take(200).ToListAsync());

    [HttpGet("{id:long}")]
    [Authorize(Roles="Admin,Security")]
    public async Task<IActionResult> Get(long id)
    {
        var rec = await _ctx.SessionRecordings.FindAsync(id);
        if (rec == null) return NotFound();
        var lines = await System.IO.File.ReadAllLinesAsync(rec.StoragePath);
        // ghép tất cả events
        var events = "[" + string.Join(",", lines.Select(l => l.Trim())) + "]";
        return File(System.Text.Encoding.UTF8.GetBytes(events), "application/json", $"session-{id}.json");
    }
}