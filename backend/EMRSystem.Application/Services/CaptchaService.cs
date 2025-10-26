// CaptchaService.cs
using System.Net.Http.Json;

namespace EMRSystem.Application.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CaptchaService> _logger;

        public CaptchaService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CaptchaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyTokenAsync(string token, string remoteIp)
        {
            var secretKey = _configuration["RecaptchaSettings:SecretKey"];
            
            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}&remoteip={remoteIp}",
                null);

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
            
            return result?.Success ?? false;
        }

        public async Task<CaptchaValidationResult> ValidateV3Async(string token, string action)
        {
            try
            {
                var secretKey = _configuration["RecaptchaSettings:SecretKey"];
                var minimumScore = double.Parse(_configuration["RecaptchaSettings:MinimumScore"]);

                var response = await _httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                    null);

                var result = await response.Content.ReadFromJsonAsync<RecaptchaV3Response>();

                if (result == null)
                {
                    return new CaptchaValidationResult { Success = false };
                }

                var isValid = result.Success && 
                             result.Score >= minimumScore && 
                             result.Action == action;

                return new CaptchaValidationResult
                {
                    Success = isValid,
                    Score = result.Score,
                    Action = result.Action,
                    ErrorCodes = result.ErrorCodes ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CAPTCHA validation error");
                return new CaptchaValidationResult { Success = false };
            }
        }
    }

    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; }
    }

    public class RecaptchaV3Response : RecaptchaResponse
    {
        public double Score { get; set; }
        public string Action { get; set; }
        public DateTime ChallengeTs { get; set; }
        public string Hostname { get; set; }
    }
}