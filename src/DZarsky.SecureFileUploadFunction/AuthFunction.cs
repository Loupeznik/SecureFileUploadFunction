using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using DZarsky.SecureFileUploadFunction.Common;
using DZarsky.SecureFileUploadFunction.Models;
using DZarsky.SecureFileUploadFunction.Services;
using DZarsky.SecureFileUploadFunction.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace DZarsky.SecureFileUploadFunction
{
    public sealed class AuthFunction
    {
        private readonly ILogger<AuthFunction> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public AuthFunction(ILogger<AuthFunction> log, IConfiguration configuration, UserService userService)
        {
            _logger = log;
            _configuration = configuration;
            _userService = userService;
        }

        [FunctionName("SignUp")]
        [OpenApiOperation(operationId: "CreateUser", tags: new[] { ApiConstants.AuthSectionName })]
        [OpenApiRequestBody(ApiConstants.JsonContentType, typeof(UserDto))]
        [OpenApiSecurity(ApiConstants.ApiKeyAuthSchemeID, SecuritySchemeType.ApiKey, In = OpenApiSecurityLocationType.Header, Name = ApiConstants.AuthApiKeyHeader)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ApiConstants.JsonContentType, bodyType: typeof(CreateUserResult), Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ApiConstants.JsonContentType, bodyType: typeof(ProblemDetails), Description = "Bad request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ApiConstants.JsonContentType, bodyType: typeof(ProblemDetails), Description = "Unauthorized")]
        public async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = $"{ApiConstants.AuthSectionName}/signup")] UserDto user, HttpRequest req)
        {
            var isApiKeyFilled = req.Headers.TryGetValue(ApiConstants.AuthApiKeyHeader, out var apiKey);

            if (!isApiKeyFilled || apiKey.FirstOrDefault() != _configuration.GetValue<string>("SignUpSecret"))
            {
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "API key is missing or incorrect",
                    Status = 401
                });
            }

            if (string.IsNullOrWhiteSpace(user.Login) || string.IsNullOrWhiteSpace(user.Password))
            {
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Bad request",
                    Detail = "Login or password were empty",
                    Status = 400
                });
            }

            var result = await _userService.CreateUser(user);

            if (result.Status == ResultStatus.AlreadyExists)
            {
                return new ConflictObjectResult(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = "Record with given username already exists",
                    Status = 429
                });
            }
            else if (result.Status == ResultStatus.Failed)
            {
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "Failed to create account",
                    Status = 400
                });
            }

            return new OkObjectResult(result.User);
        }
    }
}
