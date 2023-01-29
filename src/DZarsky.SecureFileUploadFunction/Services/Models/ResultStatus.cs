using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DZarsky.SecureFileUploadFunction.Services.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultStatus
    {
        Success,
        AlreadyExists,
        Failed,
        NotFound
    }
}
