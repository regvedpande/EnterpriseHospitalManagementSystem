using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hospital.Utilities
{
    public class ImageOperations
    {
        private readonly IWebHostEnvironment _env;

        public ImageOperations(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string ImageUpload(IFormFile file)
        {
            string fileName = null;
            if (file != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return fileName;
        }
    }
}
