[ApiController]
[Route("api/security/password-policy")]
[Authorize(Roles="Admin")]
public class PasswordPolicyController : ControllerBase
{
    private readonly IPasswordPolicyService _svc;
    public PasswordPolicyController(IPasswordPolicyService svc) { _svc = svc; }

    [HttpGet] public async Task<ActionResult<PasswordPolicy>> Get() => Ok(await _svc.GetAsync());

    [HttpPut]
    public async Task<ActionResult<PasswordPolicy>> Update([FromBody] PasswordPolicy dto)
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var updated = await _svc.UpdateAsync(dto, adminId);
        return Ok(updated);
    }
}