// SecurityController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<List<UserSession>>> GetActiveSessions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var sessions = await _securityService.GetActiveSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpPost("sessions/{sessionId}/revoke")]
        public async Task<IActionResult> RevokeSession(int sessionId)
        {
            await _securityService.RevokeSessionAsync(sessionId);
            return Ok(new { message = "Phiên đã được thu hồi" });
        }

        [HttpPost("sessions/revoke-all")]
        public async Task<IActionResult> RevokeAllSessions([FromQuery] int? exceptSessionId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _securityService.RevokeAllSessionsAsync(userId, exceptSessionId ?? 0);
            return Ok(new { message = "Đã thu hồi tất cả phiên đăng nhập" });
        }

        [HttpGet("login-history")]
        public async Task<ActionResult<List<LoginAttempt>>> GetLoginHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var history = await _context.LoginAttempts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AttemptedAt)
                .Take(50)
                .ToListAsync();
            return Ok(history);
        }
    }
}