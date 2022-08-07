using Azure.Storage.Blobs.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public class UploadFileResult
    {
        public BlobContentInfo? Result { get; set; }

        public UploadFileResultStatus Status { get; set; }

        public UploadFileResult(UploadFileResultStatus status, BlobContentInfo? result = null)
        {
            Result = result;
            Status = status;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum UploadFileResultStatus
    {
        Success,
        AlreadyExists,
        Failed
    }
}
