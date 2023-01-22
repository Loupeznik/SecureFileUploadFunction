namespace DZarsky.SecureFileUploadFunction.Common
{
    public static class CosmosConstants
    {
        public const string DatabaseID = "SecureFileUploadFunction";
        public const string ContainerID = "Users";
    }

    public static class ApiConstants
    {
        public const string AuthSectionName = "auth";
        public const string FilesSectionName = "files";
        public const string AuthApiKeyHeader = "X-SIGNUP-KEY";

        public const string BasicAuthSchemeID = "basic_auth";
        public const string ApiKeyAuthSchemeID = "api_key";

        public const string JsonContentType = "application/json";
    }
}
