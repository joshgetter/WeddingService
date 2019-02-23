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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadService.Helpers;
using UploadService.Models;

namespace UploadService.Controllers
{
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]
    public class UploadController : ControllerBase
    {
        #region Fields
        private static readonly string DROPBOXFOLDER = "11WsVkfkWBVfkwiP8ggCtaBWyLMnqPk10";
        #endregion

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
                var uploadTasks = new List<Task<IUploadProgress>>();
                foreach (var incomingFile in incomingUpload.Media)
                {
                    // Execute upload
                    uploadTasks.Add(UploadFile(incomingFile));
                }

                // Wait for all uploads to complete
                await Task.WhenAll(uploadTasks);
                return new Status { Success = uploadTasks.All(task => task.IsCompletedSuccessfully) };
            }
            catch (Exception ex)
            {
                return new Status
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <returns>The status of the file upload.</returns>
        /// <param name="incomingFile">File to upload.</param>
        private async Task<IUploadProgress> UploadFile(IFormFile incomingFile)
        {
            // Build drive file container
            var driveFile = new Google.Apis.Drive.v3.Data.File()
            {
                Name = incomingFile.FileName,
                Parents = new string[] { DROPBOXFOLDER }
            };

            using (var incomingStream = incomingFile.OpenReadStream())
            {
                // Create upload request
                var uploadRequest = DriveAuth.Service.Files.Create(driveFile,
                    incomingStream,
                    incomingFile.ContentType);

                return await uploadRequest.UploadAsync();
            }
        }
        #endregion
    }
}
