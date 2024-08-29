using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly SshFileService _sshFileService;
    private readonly ILogger<FileController> _logger;

    public FileController(SshFileService sshFileService, ILogger<FileController> logger)
    {
        _sshFileService = sshFileService;
        _logger = logger;
    }

    [HttpGet("retrieve-files")]
    public IActionResult RetrieveFiles()
    {
        try
        {
            _logger.LogInformation("Starting file retrieval from SSH server.");

            // Memanggil service untuk mengambil semua file .prt
            List<Stream> files = _sshFileService.RetrieveAllPrtFiles();

            if (files.Count == 0)
            {
                _logger.LogInformation("No .prt files found.");
                return NotFound("No .prt files found in the specified directory.");
            }

            // Mempersiapkan file untuk dikembalikan dalam respons
            var result = new List<FileContentResult>();
            foreach (var fileStream in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    var fileName = $"file_{Guid.NewGuid()}.prt"; // Nama file sementara

                    result.Add(File(memoryStream.ToArray(), "application/octet-stream", fileName));
                    fileStream.Dispose(); // Pastikan untuk membebaskan resource
                }
            }

            _logger.LogInformation("Files retrieved successfully.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving files.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
