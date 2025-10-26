// WebAuthnController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebAuthnController : ControllerBase
    {
        private readonly IWebAuthnService _webAuthnService;

        public WebAuthnController(IWebAuthnService webAuthnService)
        {
            _webAuthnService = webAuthnService;
        }

        [HttpPost("register/options")]
        [Authorize]
        public async Task<ActionResult<CredentialCreateOptions>> GetRegisterOptions()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var options = await _webAuthnService.GetCredentialOptionsAsync(userId);
                return Ok(options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var success = await _webAuthnService.RegisterCredentialAsync(userId, attestationResponse);
                
                if (success)
                    return Ok(new { message = "Biometric authentication registered successfully" });
                
                return BadRequest(new { message = "Registration failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login/options")]
        public async Task<ActionResult<AssertionOptions>> GetLoginOptions([FromBody] string email)
        {
            try
            {
                var options = await _webAuthnService.GetAssertionOptionsAsync(email);
                return Ok(options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromQuery] string email,
            [FromBody] AuthenticatorAssertionRawResponse assertionResponse)
        {
            try
            {
                var response = await _webAuthnService.VerifyAssertionAsync(email, assertionResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("credentials")]
        [Authorize]
        public async Task<ActionResult<List<WebAuthnCredential>>> GetCredentials()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var credentials = await _webAuthnService.GetUserCredentialsAsync(userId);
            return Ok(credentials);
        }

        [HttpDelete("credentials/{id}")]
        [Authorize]
        public async Task<IActionResult> RevokeCredential(int id)
        {
            await _webAuthnService.RevokeCredentialAsync(id);
            return NoContent();
        }
    }
}