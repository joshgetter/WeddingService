using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Discovery;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using UploadService.Helpers;
using UploadService.Models;

namespace UploadService.Controllers
{
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]
    public class UploadController : ControllerBase
    {
        #region Methods
        [HttpGet("IsActive")]
        public Status GetStatus()
        {
            return new Status { Success = true };
        }

        [HttpPost("UploadMedia")]
        [DisableRequestSizeLimit]
        public async Task<Status> UploadMedia([FromForm] IncomingUpload incomingUpload)
        {
            try
            {
                var driveFile = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = incomingUpload.Media.FileName,
                };

                var file = DriveAuth.Service.Files.Create(driveFile, 
                    incomingUpload.Media.OpenReadStream(), 
                    incomingUpload.Media.ContentType);

                var progress = await file.UploadAsync();
                return new Status { Success = progress.Status == UploadStatus.Completed };
            }
            catch (Exception ex)
            {
                return new Status { Success = false, Message = ex.Message };
            }
        }
        #endregion
    }
}
