using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace UploadService.Models
{
    public class IncomingUpload
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public IEnumerable<IFormFile> Media { get; set; }
    }
}
