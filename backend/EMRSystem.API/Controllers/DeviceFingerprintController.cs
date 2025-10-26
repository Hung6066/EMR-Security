// EMRSystem.API/Controllers/DeviceFingerprintController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/device-fingerprint")]
    [Authorize]
    public class DeviceFingerprintController : ControllerBase
    {
        private readonly IDeviceFingerprintService _fingerprintService;

        public DeviceFingerprintController(IDeviceFingerprintService fingerprintService)
        {
            _fingerprintService = fingerprintService;
        }

        [HttpPost("submit")]
        [AllowAnonymous] // Endpoint này có thể được gọi trước khi đăng nhập
        public async Task<IActionResult> SubmitFingerprint([FromBody] FingerprintSubmission submission)
        {
            var riskScore = await _fingerprintService.CalculateRiskScoreAsync(submission.Components, null);

            var fp = await _fingerprintService.CreateOrUpdateFingerprintAsync(submission.FingerprintHash, submission.Components, riskScore.Score);
            
            // Nếu người dùng đã đăng nhập, liên kết fingerprint với user
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _fingerprintService.AssociateUserAsync(fp.Id, userId);
            }

            return Ok(new { riskScore });
        }

        [HttpGet("trusted")]
        public async Task<IActionResult> GetTrustedDevices()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var devices = await _fingerprintService.GetTrustedDevicesAsync(userId);
            return Ok(devices);
        }

        [HttpPost("trusted/{id}/revoke")]
        public async Task<IActionResult> RevokeTrustedDevice(int id)
        {
            await _fingerprintService.RevokeTrustedDeviceAsync(id);
            return NoContent();
        }
    }

    public class FingerprintSubmission
    {
        public required string FingerprintHash { get; set; }
        public required DeviceFingerprintDto Components { get; set; }
    }
}