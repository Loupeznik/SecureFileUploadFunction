namespace DZarsky.SecureFileUploadFunction.Auth
{
    public class AuthResult
    {
        public AuthResultStatus Status { get; set; }

        public string? UserID { get; set; }

        public AuthResult(AuthResultStatus status, string? userID = null)
        {
            Status = status;
            UserID = userID;
        }
    }

    public enum AuthResultStatus
    {
        Success,
        InvalidLoginOrPassword,
        UserInactive,
        Error
    }
}
