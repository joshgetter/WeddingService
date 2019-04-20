using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UploadService.Helpers;
using UploadService.Models;

namespace UploadService.Controllers
{
    [ApiController]
    public class UploadController : ControllerBase
    {
        #region Fields
        private static string DROPBOXFOLDER;
        #endregion

        #region Initializers
        public UploadController(IConfiguration configuration)
        {
            // Record dropbox folder as a singleton
            if (string.IsNullOrWhiteSpace(DROPBOXFOLDER))
            {
                DROPBOXFOLDER = configuration["DropboxFolder"];
            }
        }
        #endregion

        #region Methods
        [HttpGet("IsActive")]
        public bool IsActive()
        {
            return true;
        }

        [HttpPost("UploadMedia")]
        [DisableRequestSizeLimit]
        public async Task<Status> UploadMedia([FromForm] IncomingUpload incomingUpload)
        {
            try
            {
                string destinationFolderID = await GetDestinationFolderID(incomingUpload);
                var uploadTasks = new List<Task<IUploadProgress>>();
                foreach (var incomingFile in incomingUpload.Media)
                {
                    // Execute upload
                    uploadTasks.Add(UploadFile(incomingFile, destinationFolderID));
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
        /// Determines the folder to file a document.
        /// </summary>
        /// <returns>The parent folder to file a given document.</returns>
        /// <param name="incomingUpload">Incoming upload.</param>
        private async Task<string> GetDestinationFolderID(IncomingUpload incomingUpload)
        {
            try
            {
                // If name is empty file in "No Name" folder.
                string targetFolderName =
                    string.IsNullOrWhiteSpace(incomingUpload?.Name) ?
                    "No Name" :
                    CleanName(incomingUpload.Name);

                var searchRequest = DriveAuth.Service.Files.List();
                /* 
                 * Find files of type "folder" where the name 
                 * equals the uploaded name and is within the dropbox folder.
                */
                searchRequest.Q = $"'{DROPBOXFOLDER}' in parents and" +
                                    " mimeType = 'application/vnd.google-apps.folder' and " +
                                    $"name = '{targetFolderName}'";

                // Execute query
                var searchResult = await searchRequest.ExecuteAsync();
                if (searchResult?.Files?.Count != 0)
                {
                    // Return existing folder if one exists.
                    return searchResult.Files.FirstOrDefault().Id;
                }
                else
                {
                    // Create a new folder for this name if one doesn't exist.
                    return await CreateFolder(targetFolderName);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while getting parent folder.");
                // On error return the root dropbox folder.
                return DROPBOXFOLDER;
            }
        }

        /// <summary>
        /// Cleans the name.
        /// </summary>
        /// <returns>The clean name in Pascal (title) case.</returns>
        /// <param name="name">The raw name.</param>
        private string CleanName(string name)
        {
            TextInfo textInfo = new CultureInfo("en-us", false).TextInfo;
            return textInfo.ToTitleCase(name.Trim());
        }

        /// <summary>
        /// Create a google drive folder.
        /// </summary>
        /// <returns>The generated folder id.</returns>
        /// <param name="folderName">Folder name.</param>
        private async Task<string> CreateFolder(string folderName)
        {
            // Build folder object.
            var folderData = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new string[] { DROPBOXFOLDER }
            };
            // Build request to add folder.
            var request = DriveAuth.Service.Files.Create(folderData);
            // Request folder ID on upload.
            request.Fields = "id";
            var uploadedFolder = await request.ExecuteAsync();
            return uploadedFolder.Id;
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <returns>The status of the file upload.</returns>
        /// <param name="incomingFile">File to upload.</param>
        /// <param name="parentFileID">The parent folder (also a Google Drive "file").</param>
        private async Task<IUploadProgress> UploadFile(IFormFile incomingFile, string parentFileID)
        {
            // Build drive file container
            var driveFile = new Google.Apis.Drive.v3.Data.File()
            {
                Name = incomingFile.FileName,
                Parents = new string[] { parentFileID }
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
