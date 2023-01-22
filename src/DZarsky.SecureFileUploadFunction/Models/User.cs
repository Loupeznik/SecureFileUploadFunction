using System;
using Newtonsoft.Json;

namespace DZarsky.SecureFileUploadFunction.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        public string? Login { get; set; }

        public string? Password { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
