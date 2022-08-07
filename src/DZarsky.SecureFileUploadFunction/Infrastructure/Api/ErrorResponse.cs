namespace DZarsky.SecureFileUploadFunction.Infrastructure.Api
{
    public class ErrorResponse
    {
        public string Message { get; set; } = "An error has occured";

        public string Exception { get; set; }

        public ErrorResponse()
        {

        }

        public ErrorResponse(string exception) => Exception = exception;

        public ErrorResponse(string message, string exception)
        {
            Exception = exception;
            Message = message;
        }
    }
}
