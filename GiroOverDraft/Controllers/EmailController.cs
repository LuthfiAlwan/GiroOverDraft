using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly SshFileService _sshFileService;
    private readonly EmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(SshFileService sshFileService, EmailService emailService, ILogger<EmailController> logger)
    {
        _sshFileService = sshFileService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("send-reports")]
    public IActionResult SendReports()
    {
        try
        {
                    Task.Run(() => _emailService.SendEmailWithAttachment());
            //_logger.LogInformation("Retrieving files from SSH server.");
            //var files = _sshFileService.RetrieveAllPrtFiles();

            //if (files.Count == 0)
            //{
            //    _logger.LogInformation("No .prt files found.");
            //    return NotFound("No .prt files found in the specified directory.");
            //}

            //foreach (var fileStream in files)
            //{
            //    try
            //    {
            //        //string fileName = $"file_{Guid.NewGuid()}.prt"; // Nama file sementara, bisa diganti dengan yang lebih deskriptif
            //        //DateTime fileDate = DateTime.Now; // Atur ini dengan tanggal file sebenarnya jika tersedia
            //        //_emailService.SendEmailWithAttachment(fileStream, fileName, fileDate);
            //    }
            //    finally
            //    {
            //        fileStream.Dispose(); // Pastikan untuk membebaskan resource
            //    }
            //}

            _logger.LogInformation("All reports sent successfully.");
            return Ok("All reports sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending reports.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
