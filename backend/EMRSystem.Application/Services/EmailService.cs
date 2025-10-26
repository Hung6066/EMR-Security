// EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace EMRSystem.Application.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            var verificationLink = $"http://localhost:4200/verify-email?token={token}";
            
            var body = $@"
                <h2>Xác thực Email</h2>
                <p>Xin chào,</p>
                <p>Vui lòng click vào link bên dưới để xác thực email của bạn:</p>
                <p><a href='{verificationLink}'>Xác thực Email</a></p>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <p>Nếu bạn không yêu cầu xác thực này, vui lòng bỏ qua email này.</p>
            ";

            await SendEmailAsync(email, "Xác thực Email - EMR System", body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = $"http://localhost:4200/reset-password?token={token}";
            
            var body = $@"
                <h2>Đặt lại Mật khẩu</h2>
                <p>Xin chào,</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu. Click vào link bên dưới:</p>
                <p><a href='{resetLink}'>Đặt lại Mật khẩu</a></p>
                <p>Link này sẽ hết hạn sau 1 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            ";

            await SendEmailAsync(email, "Đặt lại Mật khẩu - EMR System", body);
        }

        public async Task SendSecurityAlertEmailAsync(string email, string alertType, string details)
        {
            var body = $@"
                <h2>Cảnh báo Bảo mật</h2>
                <p>Xin chào,</p>
                <p><strong>{alertType}</strong></p>
                <p>{details}</p>
                <p>Nếu đây không phải là bạn, vui lòng đổi mật khẩu ngay lập tức và liên hệ với chúng tôi.</p>
            ";

            await SendEmailAsync(email, $"Cảnh báo Bảo mật - {alertType}", body);
        }
    }
}