using Renci.SshNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet.Common;

public class SshFileService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SshFileService> _logger;

    public SshFileService(IConfiguration configuration, ILogger<SshFileService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public List<Stream> RetrieveAllPrtFiles()
    {
        var sshSettings = _configuration.GetSection("SshSettings");
        var directoryPath = sshSettings["DirectoryPath"];
        var files = new List<Stream>();

        using (var client = new SftpClient(sshSettings["Host"], int.Parse(sshSettings["Port"]), sshSettings["Username"], sshSettings["Password"]))
        {
            try
            {
                _logger.LogInformation("Connecting to SSH server...");
                client.Connect();
                Console.WriteLine("Server");
                _logger.LogInformation("Successfully connected to SSH server.");

                // Mengambil semua file .prt dari direktori yang ditentukan
                var fileList = client.ListDirectory(directoryPath)
                                     .Where(file => file.Name.EndsWith(".prt") && !file.IsDirectory)
                                     .ToList();

                _logger.LogInformation($"Found {fileList.Count} .prt files in directory {directoryPath}.");

                foreach (var file in fileList)
                {
                    var memoryStream = new MemoryStream();
                    client.DownloadFile(file.FullName, memoryStream);
                    memoryStream.Position = 0; // Reset posisi stream ke awal
                    files.Add(memoryStream);
                    _logger.LogInformation($"File '{file.Name}' downloaded successfully.");
                }
            }
            catch (SshException sshEx)
            {
                _logger.LogError(sshEx, "SSH connection error.");
                throw; // Melempar kembali pengecualian untuk dihandle di tempat lain
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "I/O error during file download.");
                throw; // Melempar kembali pengecualian untuk dihandle di tempat lain
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                throw; // Melempar kembali pengecualian untuk dihandle di tempat lain
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                    _logger.LogInformation("Disconnected from SSH server.");
                }
            }
        }

        return files;
    }
}
