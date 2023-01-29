using DZarsky.SecureFileUploadFunction.Models;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    public sealed class UserInfoResult
    {
        public User? User { get; set; }

        public ResultStatus Status { get; set; }

        public UserInfoResult(ResultStatus status, User? user = null)
        {
            User = user;
            Status = status;
        }
    }
}
