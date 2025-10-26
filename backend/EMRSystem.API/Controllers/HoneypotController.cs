using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    // Các endpoint này không được quảng bá và không được frontend sử dụng.
    // Chúng được thiết kế để bẫy các công cụ quét tự động.
    [ApiController]
    [AllowAnonymous] // Cho phép truy cập không cần xác thực để bẫy cả kẻ tấn công bên ngoài
    [Route("wp-admin")] // Giả dạng một trang admin phổ biến
    public class HoneypotController : ControllerBase
    {
        private readonly IDeceptionService _deceptionService;

        public HoneypotController(IDeceptionService deceptionService)
        {
            _deceptionService = deceptionService;
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [Route("{*url}")] // Bắt tất cả các request đến /wp-admin
        public async Task<IActionResult> Trap()
        {
            await _deceptionService.TriggerHoneypotAsync(
                "Honeypot-Endpoint",
                $"Access attempt to a decoy endpoint: {Request.Path}");
            
            // Trì hoãn response để làm chậm kẻ tấn công
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            // Trả về lỗi 404 Not Found chung chung
            return NotFound();
        }
    }
}