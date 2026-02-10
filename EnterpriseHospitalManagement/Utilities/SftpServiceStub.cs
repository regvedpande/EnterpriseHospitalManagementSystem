using Microsoft.Extensions.Configuration;

namespace Hospital.Utilities
{
    // Development stub: returns false and avoids needing Renci.SshNet until you can install it.
    public class SftpService
    {
        private readonly IConfiguration _config;
        public SftpService(IConfiguration config) => _config = config;

        public bool Upload(string localFilePath)
        {
            // no-op fallback for development
            return false;
        }
    }
}
