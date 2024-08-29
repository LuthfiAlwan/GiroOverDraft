using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    //public void SendEmailWithAttachment(Stream fileStream, string fileName, DateTime fileDate)
    //{
    //    var emailSettings = _configuration.GetSection("EmailSettings");

    //    string smtpServer = emailSettings["SmtpServer"];
    //    string portString = emailSettings["Port"];
    //    string username = emailSettings["Username"];
    //    string password = emailSettings["Password"];
    //    string fromEmail = emailSettings["FromEmail"];
    //    bool enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

    //    _logger.LogInformation($"SMTP Server: {smtpServer}");
    //    _logger.LogInformation($"SMTP Port: {portString}");
    //    _logger.LogInformation($"SMTP Username: {username}");
    //    _logger.LogInformation($"SMTP From Email: {fromEmail}");
    //    _logger.LogInformation($"SMTP Enable SSL: {enableSsl}");

    //    if (string.IsNullOrEmpty(portString))
    //    {
    //        throw new ArgumentNullException(nameof(portString), "SMTP port configuration is missing.");
    //    }

    //    if (!int.TryParse(portString, out int port))
    //    {
    //        throw new ArgumentException("Invalid SMTP port number.");
    //    }

    //    var smtpClient = new SmtpClient(smtpServer)
    //    {
    //        Port = port,
    //        Credentials = new NetworkCredential(username, password),
    //        EnableSsl = enableSsl
    //    };

    //    var mailMessage = new MailMessage
    //    {
    //        From = new MailAddress(fromEmail),
    //        Subject = $"Report Bulk Upload Limit BPJS [{fileDate:ddMMyyyy}]",
    //        Body = $"Dear Team,\n\nBerikut report \"{fileName}\" atas Setting Limit Giro Overdraft pada tanggal \"{fileDate:dd/MM/yyyy}\"\n\nDemikian disampaikan, terima kasih atas perhatian dan kerjasamanya.",
    //        IsBodyHtml = false
    //    };

    //    mailMessage.To.Add(emailSettings["ToEmail"]);

    //    fileStream.Position = 0;
    //    mailMessage.Attachments.Add(new Attachment(fileStream, fileName));

    //    try
    //    {
    //        smtpClient.Send(mailMessage);
    //        _logger.LogInformation("Email sent successfully.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to send email.");
    //        throw;
    //    }
    //}

    public void SendEmailWithAttachment()
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        string smtpServer = emailSettings["SmtpServer"];
        string portString = emailSettings["Port"];
        string username = emailSettings["Username"];
        string password = emailSettings["Password"];
        string fromEmail = emailSettings["FromEmail"];
        string toEmail = emailSettings["ToEmail"];
        bool enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

        _logger.LogInformation($"SMTP Server: {smtpServer}");
        _logger.LogInformation($"SMTP Port: {portString}");
        _logger.LogInformation($"SMTP Username: {username}");
        _logger.LogInformation($"SMTP From Email: {fromEmail}");
        _logger.LogInformation($"SMTP Enable SSL: {enableSsl}");

        if (string.IsNullOrEmpty(portString))
        {
            throw new ArgumentNullException(nameof(portString), "SMTP port configuration is missing.");
        }

        if (!int.TryParse(portString, out int port))
        {
            throw new ArgumentException("Invalid SMTP port number.");
        }

        using (var client = new SmtpClient(smtpServer, int.Parse(portString)))
        {
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(username, password);
            var subject = "INVOICE";
            var body ="Body";


            var message = new MailMessage(username, toEmail, subject, body);
            message.IsBodyHtml = true;
            client.Send(message);

        }
    }
}
