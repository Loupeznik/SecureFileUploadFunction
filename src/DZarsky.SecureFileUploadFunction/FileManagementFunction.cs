using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using DZarsky.SecureFileUploadFunction.Auth;
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
    public class FileManagementFunction
    {
        private readonly ILogger<FileManagementFunction> _logger;
        private readonly IConfiguration _configuration;
        private readonly AuthManager _authManager;

        private const string _section = "Files";

        public FileManagementFunction(ILogger<FileManagementFunction> logger, IConfiguration configuration, AuthManager authManager)
        {
            _logger = logger;
            _configuration = configuration;
            _authManager = authManager;
        }

        [FunctionName("Upload")]
        [OpenApiOperation(operationId: "UploadFile", tags: new[] { _section })]
        [OpenApiSecurity("basic_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Basic)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Description = "File upload")]
        public async Task<ActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")] HttpRequest req)
        {
            var authHeader = req.Headers["Authorization"];

            var credentials = AuthManager.ParseToken(authHeader);

            if (credentials == null)
            {
                return new UnauthorizedResult();
            }

            var authResult = await _authManager.ValidateCredentials(credentials);

            if (authResult.Status != AuthResultStatus.Success)
            {
                return new UnauthorizedResult();
            }

            try
            {
                var file = req.Form.Files["file"];

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                var blobClient = new BlobClient(
                    _configuration.GetValue<string>("UploadEndpoint"),
                    authResult.UserID,
                    file.FileName);

                var result = await blobClient.UploadAsync(memoryStream);

                return new OkObjectResult(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not upload file", ex);

                return new BadRequestObjectResult(new
                {
                    Message = "An error has accured",
                    Exception = ex.Message
                });
            }
        }
    }
}
