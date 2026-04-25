using System.Text;
using System.Text.Json;
using GymSaaS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymSaaS.Infrastructure.Services;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<BrevoEmailService> _logger;

    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    public BrevoEmailService(HttpClient httpClient, IConfiguration config, ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _config     = config;
        _logger     = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey      = _config["Brevo:ApiKey"]      ?? throw new InvalidOperationException("Brevo:ApiKey is not configured.");
        var senderEmail = _config["Brevo:SenderEmail"] ?? throw new InvalidOperationException("Brevo:SenderEmail is not configured.");
        var senderName  = _config["Brevo:SenderName"]  ?? "Gym System";

        var body = new
        {
            sender     = new { email = senderEmail, name = senderName },
            to         = new[] { new { email = toEmail } },
            subject,
            htmlContent,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BrevoApiUrl);
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Brevo email failed. Status: {Status}. Body: {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Brevo email failed ({response.StatusCode}): {error}");
        }

        _logger.LogInformation("Email sent to {Email} — subject: {Subject}", toEmail, subject);
    }

    public async Task SendPasswordSetupAsync(string toEmail, string recipientName, string setupLink)
    {
        var senderName = _config["Brevo:SenderName"] ?? "Gym System";

        var html = $@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif; background: #f4f4f4; padding: 32px;"">
  <div style=""max-width: 560px; margin: auto; background: #ffffff; border-radius: 12px; padding: 40px;"">
    <h2 style=""color: #E53E3E; margin-bottom: 8px;"">Welcome to {senderName}</h2>
    <p style=""color: #555; font-size: 15px;"">Hi {recipientName},</p>
    <p style=""color: #555; font-size: 15px;"">Your account has been created. Click the button below to set your password and activate your account.</p>
    <div style=""text-align: center; margin: 32px 0;"">
      <a href=""{setupLink}"" style=""background: #E53E3E; color: #fff; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 15px;"">
        Set My Password
      </a>
    </div>
    <p style=""color: #999; font-size: 12px;"">This link expires in 24 hours. If you did not expect this email, please ignore it.</p>
  </div>
</body>
</html>";

        await SendAsync(toEmail, $"Set Your Password — {senderName}", html);
    }

    public async Task SendCredentialsAsync(string toEmail, string recipientName, string username, string temporaryPassword)
    {
        var senderName = _config["Brevo:SenderName"] ?? "Gym System";

        var html = $@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif; background: #f4f4f4; padding: 32px;"">
  <div style=""max-width: 560px; margin: auto; background: #ffffff; border-radius: 12px; padding: 40px;"">
    <h2 style=""color: #E53E3E; margin-bottom: 8px;"">Your Account is Ready</h2>
    <p style=""color: #555; font-size: 15px;"">Hi {recipientName},</p>
    <p style=""color: #555; font-size: 15px;"">Your account for <strong>{senderName}</strong> has been created. Here are your login credentials:</p>
    <table style=""width: 100%; border-collapse: collapse; margin: 24px 0;"">
      <tr>
        <td style=""padding: 10px 16px; background: #f9f9f9; border: 1px solid #eee; font-weight: bold; color: #333; width: 40%;"">Username</td>
        <td style=""padding: 10px 16px; background: #f9f9f9; border: 1px solid #eee; color: #555;"">{username}</td>
      </tr>
      <tr>
        <td style=""padding: 10px 16px; border: 1px solid #eee; font-weight: bold; color: #333;"">Temporary Password</td>
        <td style=""padding: 10px 16px; border: 1px solid #eee; color: #555; font-family: monospace; font-size: 15px;"">{temporaryPassword}</td>
      </tr>
    </table>
    <p style=""color: #E53E3E; font-size: 13px;"">Please change your password immediately after logging in.</p>
    <p style=""color: #999; font-size: 12px;"">If you did not expect this email, please contact your administrator.</p>
  </div>
</body>
</html>";

        await SendAsync(toEmail, $"Your Account Credentials — {senderName}", html);
    }
}
