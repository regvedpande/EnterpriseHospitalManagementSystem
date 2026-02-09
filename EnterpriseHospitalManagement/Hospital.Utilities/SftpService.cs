using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.IO;

namespace Hospital.Utilities
{
    public class SftpService
    {
        private readonly IConfiguration _config;

        public SftpService(IConfiguration config)
        {
            _config = config;
        }

        public bool Upload(string localFilePath)
        {
            var sftp = _config.GetSection("Sftp");
            var host = sftp.GetValue<string>("Host");
            var port = sftp.GetValue<int>("Port");
            var username = sftp.GetValue<string>("Username");
            var password = sftp.GetValue<string>("Password");
            var remotePath = sftp.GetValue<string>("RemotePath");

            if (string.IsNullOrEmpty(host)) return false;

            using var client = new SftpClient(host, port, username, password);
            client.Connect();
            if (!client.IsConnected) return false;

            using var fs = new FileStream(localFilePath, FileMode.Open);
            var remoteFileName = Path.Combine(remotePath, Path.GetFileName(localFilePath)).Replace("\\", "/");
            client.UploadFile(fs, remoteFileName);
            client.Disconnect();
            return true;
        }
    }
}
