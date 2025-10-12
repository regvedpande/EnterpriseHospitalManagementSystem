using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hospital.Utilities
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
            string filename = null;
            if (file != null)
            {
                filename = file.FileName;
                string fileDirectory = Path.Combine(_env.WebRootPath, "Images");
                string filePath = Path.Combine(fileDirectory, filename);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return filename;
        }
    }
}
