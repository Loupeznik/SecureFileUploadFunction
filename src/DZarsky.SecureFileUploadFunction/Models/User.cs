using System;
using System.Text.Json.Serialization;

namespace DZarsky.SecureFileUploadFunction.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        public string? Login { get; set; }

        public string? Password { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
