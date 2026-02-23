using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.IO;

namespace Hospital.Utilities
{
    /// <summary>
    /// Simple SFTP upload/download helper backed by SSH.NET (Renci.SshNet).
    /// Configure via appsettings.json "Sftp" section.
    /// </summary>
    public class SftpService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SftpService> _logger;

        public SftpService(IConfiguration config, ILogger<SftpService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private SftpClient CreateClient()
        {
            var section = _config.GetSection("Sftp");
            var host = section.GetValue<string>("Host") ?? "localhost";
            var port = section.GetValue<int>("Port");
            if (port <= 0) port = 22;
            var username = section.GetValue<string>("Username") ?? "";
            var password = section.GetValue<string>("Password") ?? "";
            return new SftpClient(host, port, username, password);
        }

        public bool UploadFile(string localPath, string remotePath)
        {
            try
            {
                using var client = CreateClient();
                client.Connect();
                using var fs = File.OpenRead(localPath);
                // SSH.NET UploadFile signature: UploadFile(Stream input, string path, bool canOverride = false)
                client.UploadFile(fs, remotePath, true);
                client.Disconnect();
                _logger.LogInformation("SFTP upload succeeded: {LocalPath} -> {RemotePath}", localPath, remotePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SFTP upload failed: {LocalPath} -> {RemotePath}", localPath, remotePath);
                return false;
            }
        }

        public bool DownloadFile(string remotePath, string localPath)
        {
            try
            {
                using var client = CreateClient();
                client.Connect();
                using var fs = File.Create(localPath);
                client.DownloadFile(remotePath, fs);
                client.Disconnect();
                _logger.LogInformation("SFTP download succeeded: {RemotePath} -> {LocalPath}", remotePath, localPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SFTP download failed: {RemotePath} -> {LocalPath}", remotePath, localPath);
                return false;
            }
        }
    }
}