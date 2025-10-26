// EMRSystem.API/Controllers/ApiKeysController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/apikeys")]
    [Authorize] // Yêu cầu đăng nhập để quản lý keys
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeysController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserApiKeys()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var keys = await _apiKeyService.GetUserApiKeysAsync(userId);
            // Không trả về KeyHash để bảo mật
            var result = keys.Select(k => new {
                k.Id, k.Name, k.KeyPrefix, k.CreatedAt, k.ExpiresAt, k.LastUsedAt, k.IsActive, k.IsRevoked
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var apiKeyResponse = await _apiKeyService.CreateApiKeyAsync(userId, dto);
            // Chỉ trả về key đầy đủ một lần duy nhất
            return Ok(apiKeyResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RevokeApiKey(int id)
        {
            // Thêm logic kiểm tra xem user có quyền xóa key này không
            await _apiKeyService.RevokeApiKeyAsync(id);
            return NoContent();
        }
    }
}