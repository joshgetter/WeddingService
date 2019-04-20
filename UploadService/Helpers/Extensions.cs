using System;
using System.IO;

namespace UploadService.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Converts the string to a stream.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="str">The string.</param>
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
