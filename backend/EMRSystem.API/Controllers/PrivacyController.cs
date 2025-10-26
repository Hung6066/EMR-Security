// EMRSystem.API/Controllers/PrivacyController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/privacy")]
    [Authorize(Roles = "Admin,Security,DataScientist")] // Giới hạn quyền truy cập
    public class PrivacyController : ControllerBase
    {
        private readonly IDataAnonymizationService _privacyService;

        public PrivacyController(IDataAnonymizationService privacyService)
        {
            _privacyService = privacyService;
        }

        [HttpPost("anonymize")]
        public async Task<IActionResult> AnonymizeData([FromBody] AnonymizeRequest request)
        {
            var result = await _privacyService.AnonymizeAsync(request.Data);
            return Ok(result);
        }

        [HttpPost("pseudonymize")]
        public async Task<IActionResult> PseudonymizeData([FromBody] PseudonymizeRequest request)
        {
            var result = await _privacyService.PseudonymizeAsync(request.Data, request.Salt);
            return Ok(result);
        }

        [HttpPost("synthetic")]
        public async Task<IActionResult> GenerateSyntheticData([FromBody] SyntheticDataRequest request)
        {
            var result = request.DataType switch
            {
                "Patient" => await _privacyService.GenerateSyntheticDataAsync<Patient>(request.RecordCount),
                // Thêm các case khác nếu cần
                _ => throw new NotSupportedException($"Data type '{request.DataType}' is not supported for synthetic generation.")
            };
            return Ok(result);
        }

        [HttpPost("tokenize")]
        public async Task<IActionResult> Tokenize([FromBody] TokenizeRequest request)
        {
            var token = await _privacyService.TokenizeAsync(request.Data, request.TokenType);
            return Ok(new { token });
        }

        [HttpPost("detokenize")]
        public async Task<IActionResult> Detokenize([FromBody] DetokenizeRequest request)
        {
            var originalData = await _privacyService.DetokenizeAsync(request.Token);
            return Ok(new { originalData });
        }

        [HttpPost("differential-privacy")]
        public async Task<IActionResult> ApplyDifferentialPrivacy([FromBody] DpRequest request)
        {
            var result = await _privacyService.ApplyDifferentialPrivacyAsync(request.Query, request.Epsilon);
            return Ok(result);
        }

        // --- Request DTOs for this controller ---
        public record AnonymizeRequest(object Data);
        public record PseudonymizeRequest(object Data, string Salt);
        public record SyntheticDataRequest(string DataType, int RecordCount);
        public record TokenizeRequest(string Data, string TokenType);
        public record DetokenizeRequest(string Token);
        public record DpRequest(string Query, double Epsilon);
    }
}