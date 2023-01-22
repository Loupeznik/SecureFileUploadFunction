namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public sealed class BlobResultWrapper<TResult> where TResult : class
    {
        public TResult? Result { get; set; }

        public ResultStatus Status { get; set; }

        public string? ErrorMessage { get; set; }

        public BlobResultWrapper(ResultStatus status, string? errorMessage = null, TResult? result = null)
        {
            Result = result;
            Status = status;
            ErrorMessage = errorMessage;
        }
    }
}
