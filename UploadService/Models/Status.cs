using System;
namespace UploadService.Models
{
    public class Status
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:UploadService.Models.Status"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}
