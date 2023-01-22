using DZarsky.SecureFileUploadFunction.Models;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public sealed class CreateUserResult
    {
        public User? User { get; set; }

        public ResultStatus Status { get; set; }

        public CreateUserResult(ResultStatus status, User? user = null)
        {
            User = user;
            Status = status;
        }
    }
}
