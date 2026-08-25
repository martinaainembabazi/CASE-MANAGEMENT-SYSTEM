using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Template.Core.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpServer = _config["SmtpSettings:Server"];
        var port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
        var senderEmail = _config["SmtpSettings:SenderEmail"];
        var senderPassword = _config["SmtpSettings:SenderPassword"];

        using var message = new MailMessage();
        message.From = new MailAddress(senderEmail!, "Bank of Uganda - CMS");
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(smtpServer, port)
        {
            UseDefaultCredentials = false, // MUST be set before Credentials
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}