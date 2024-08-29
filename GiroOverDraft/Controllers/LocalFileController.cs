using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class LocalFileController : ControllerBase
{
    private readonly LocalFileService _localFileService;
    private readonly EmailService _emailService;
    private readonly ILogger<LocalFileController> _logger;

    public LocalFileController(LocalFileService localFileService, ILogger<LocalFileController> logger)
    {
        _localFileService = localFileService;
        _logger = logger;
    }

    [HttpGet("send-local-reports")]
    public IActionResult SendLocalReports()
    {
        try
        {
            _logger.LogInformation("Retrieving files from local directory.");
            var files = _localFileService.RetrieveAllPrtFiles();

            if (files.Count == 0)
            {
                _logger.LogInformation("No .prt files found.");
                return NotFound("No .prt files found in the specified directory.");
            }

    
                try
                {
                    Task.Run(() => _localFileService.SendEmailWithAttachment(files));
                    //_localFileService.SendEmailWithAttachment(fileInfo);
                    _logger.LogInformation($"Report sent successfully for file {files}.");
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, $"Failed to send email for file {files.Name}.");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            

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
