using Azure.Storage.Blobs.Models;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public class UploadFileResult
    {
        public BlobContentInfo? Result { get; set; }

        public ResultStatus Status { get; set; }

        public UploadFileResult(ResultStatus status, BlobContentInfo? result = null)
        {
            Result = result;
            Status = status;
        }
    }
}
