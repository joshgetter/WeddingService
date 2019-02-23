using System;
using Microsoft.AspNetCore.Http;

namespace UploadService.Models
{
    public class IncomingUpload
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public IFormFile Media { get; set; }
    }
}
