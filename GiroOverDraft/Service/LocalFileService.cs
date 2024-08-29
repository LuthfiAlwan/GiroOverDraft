using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

public class LocalFileService
{
    private readonly IConfiguration _configuration;

    public LocalFileService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<FileInfo> RetrieveAllPrtFiles()
    {
        var localSettings = _configuration.GetSection("LocalSettings");
        var directoryPath = localSettings["DirectoryPath"];
        var files = new List<FileInfo>();

        // Check if the directory exists
        if (Directory.Exists(directoryPath))
        {
            // Retrieve all .prt files from the directory
            var fileList = Directory.GetFiles(directoryPath, "*.prt");

            foreach (var filePath in fileList)
            {
                files.Add(new FileInfo(filePath));
            }
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory {directoryPath} not found.");
        }

        return files;
    }

    public void SendEmailWithAttachment(List<FileInfo>? files)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        string smtpServer = emailSettings["SmtpServer"];
        string portString = emailSettings["Port"];
        string username = emailSettings["Username"];
        string password = emailSettings["Password"];
        string fromEmail = emailSettings["FromEmail"];
        string toEmail = emailSettings["ToEmail"];
        bool enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

        if (string.IsNullOrEmpty(portString))
        {
            throw new ArgumentNullException(nameof(portString), "SMTP port configuration is missing.");
        }

        if (!int.TryParse(portString, out int port))
        {
            throw new ArgumentException("Invalid SMTP port number.");
        }

        if (files == null || !files.Any())
        {
            throw new ArgumentException("No files provided for attachment.");
        }

        // Assuming all files have the same base name format, use the first file's name
        var firstFile = files.First();
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(firstFile.Name);

        // Extract date from file name assuming format onltrx7010.yyyyMMdd.HHmmss.TXXXX.SXXXXX
        string datePart = fileNameWithoutExtension.Substring(11, 8); // Extract yyyyMMdd
        string baseFileName = fileNameWithoutExtension.Substring(0, 10); // Extract base file name part

        DateTime fileDate;
        if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out fileDate))
        {
            throw new FormatException("Date extraction from filename failed.");
        }

        string formattedDate = fileDate.ToString("dd MMM yyyy");

        var subject = $"Report Bulk Upload Limit BPJS [{fileDate:ddMMyyyy}]";

        var body = $@"
            <p>Dear Team,</p>
            <p>Berikut report {baseFileName} atas Setting Limit Giro Overdraft pada tanggal {formattedDate}.</p>
            <p>Demikian disampaikan, terima kasih atas perhatian dan kerjasamanya.</p>";

        using (var client = new SmtpClient(smtpServer, port))
        {
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(username, password);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            message.CC.Add("novayulianti0@gmail.com");
            message.CC.Add("luthfialwannrama@gmail.com");

            // Add attachments
            foreach (var file in files)
            {
                if (file.Exists)
                {
                    var attachment = new Attachment(file.FullName);
                    message.Attachments.Add(attachment);
                }
            }

            client.Send(message);

            // Dispose of the attachments
            foreach (var attachment in message.Attachments)
            {
                attachment.Dispose();
            }
        }
    }

}
