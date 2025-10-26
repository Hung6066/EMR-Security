// IEmailService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string email, string token);
        Task SendPasswordResetEmailAsync(string email, string token);
        Task SendSecurityAlertEmailAsync(string email, string alertType, string details);
    }
}