using System.IO;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public class GetFileResult
    {
        public string TemporaryPath { get; set; }

        public string ContentType { get; set; }

        public GetFileResult(string tempPath, string contentType)
        {
            TemporaryPath = tempPath;
            ContentType = contentType;
        }
    }
}
