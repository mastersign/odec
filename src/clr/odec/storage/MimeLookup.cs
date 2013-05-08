using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.odec.storage
{
    internal static class MimeLookup
    {
        /// <summary>
        /// Retrieves the MIME type for a file extension.
        /// </summary>
        /// <param name="fileExt">The file extension.</param>
        /// <returns>The MIME type.</returns>
        public static string LookupMimeType(string fileExt)
        {
            switch (fileExt)
            {
                // Container specific
                case "sig":
                    return "text/x-signature+xml";

                // Text
                case "txt":
                case "ini":
                    return "text/plain";
                case "xml":
                    return "text/xml";
                case "tsv":
                    return "text/tab-separated-values";
                case "csv":
                    return "text/csv";

                // Images
                case "png":
                    return "image/png";
                case "jpeg":
                case "jpg":
                case "jpe":
                    return "image/jpeg";
                case "tiff":
                case "tif":
                    return "image/tiff";
                case "gif":
                    return "image/gif";
                case "svg":
                    return "image/svg+xml";
                case "bmp":
                    return "image/bmp";

                // Audio
                case "au":
                case "snd":
                    return "audio/basic";
                case "wav":
                    return "audio/x-wav";
                case "aif":
                case "aiff":
                case "aifc":
                    return "audio/x-aiff";
                case "mpa":
                    return "audio/mpeg";

                // Video
                case "mp2":
                case "mpe":
                case "mpeg":
                case "mpg":
                case "mpv2":
                    return "video/mpeg";
                case "mov":
                    return "video/quicktime";
                case "avi":
                    return "video/x-msvideo";

                // Web
                case "htm":
                case "html":
                case "shtml":
                    return "text/html";
                case "xhtml":
                    return "text/xhtml+xml";
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";

                // MS Office
                case "doc":
                    return "application/msword";
                case "xls":
                    return "application/msexcel";
                case "ppt":
                    return "application/mspowerpoint";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                // Archives
                case "zip":
                    return "application/zip";
                case "gz":
                case "gzip":
                    return "application/gzip";
                case "gtar":
                    return "application/x-gtar";

                // Miscellaneous
                case "exe":
                    return "application/octet-stream";
                case "rtf":
                    return "text/rtf";
                case "rtx":
                    return "text/richtext";
                case "ps":
                    return "application/postscript";

                // Else
                default:
                    return "application/unknown";
            }
        }
    }
}
