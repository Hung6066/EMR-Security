// TwoFactorController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;

        public TwoFactorController(ITwoFactorService twoFactorService)
        {
            _twoFactorService = twoFactorService;
        }

        [HttpPost("enable")]
        public async Task<ActionResult<Enable2FADto>> Enable()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _twoFactorService.EnableTwoFactorAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] Verify2FADto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isValid = await _twoFactorService.VerifyAndEnableTwoFactorAsync(userId, dto.Code);
                
                if (!isValid)
                    return BadRequest(new { message = "Mã xác thực không đúng" });

                return Ok(new { message = "Đã bật xác thực 2 bước thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromBody] string password)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _twoFactorService.DisableTwoFactorAsync(userId, password);
                
                if (!result)
                    return BadRequest(new { message = "Mật khẩu không đúng" });

                return Ok(new { message = "Đã tắt xác thực 2 bước" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("regenerate-backup-codes")]
        public async Task<ActionResult<List<string>>> RegenerateBackupCodes()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var codes = await _twoFactorService.RegenerateBackupCodesAsync(userId);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}