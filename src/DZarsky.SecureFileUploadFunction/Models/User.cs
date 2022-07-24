using System;

namespace DZarsky.SecureFileUploadFunction.Models
{
    public class User
    {
        public string? Login { get; set; }

        public string? Password { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
