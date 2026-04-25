namespace GymSaaS.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlContent);
    Task SendPasswordSetupAsync(string toEmail, string recipientName, string setupLink);
    Task SendCredentialsAsync(string toEmail, string recipientName, string username, string temporaryPassword);
}
