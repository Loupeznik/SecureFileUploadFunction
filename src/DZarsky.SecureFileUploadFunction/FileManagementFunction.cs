using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using DZarsky.CommonLibraries.AzureFunctions.Models.Auth;
using DZarsky.CommonLibraries.AzureFunctions.Security;
using DZarsky.SecureFileUploadFunction.Common;
using DZarsky.SecureFileUploadFunction.Infrastructure.Api;
using DZarsky.SecureFileUploadFunction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace DZarsky.SecureFileUploadFunction;

public sealed class FileManagementFunction
{
    private readonly ILogger<FileManagementFunction> _logger;
    private readonly FileService _fileService;
    private readonly IAuthManager _authManager;

    public FileManagementFunction(ILogger<FileManagementFunction> logger, FileService fileService, IAuthManager authManager)
    {
        _logger = logger;
        _fileService = fileService;
        _authManager = authManager;
    }

    [FunctionName("Upload")]
    [OpenApiOperation(operationId: "UploadFile", tags: new[] { ApiConstants.FilesSectionName })]
    [OpenApiSecurity(ApiConstants.BasicAuthSchemeId, SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Basic)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ApiConstants.JsonContentType, bodyType: typeof(Response<BlobContentInfo>), Description = "File upload")]
    public async Task<ActionResult> UploadFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = $"{ApiConstants.FilesSectionName}/upload")] HttpRequest req)
    {
        var authResult = await Authorize(req);

        if (authResult.Status != AuthResultStatus.Success)
        {
            return new UnauthorizedResult();
        }

        try
        {
            var file = req.Form.Files["file"];

            if (file is null || file.Length < 1)
            {
                return new BadRequestResult();
            }

            var result = await _fileService.UploadFile(file, authResult.UserID);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not upload file - {Exception}", ex);

            return new BadRequestObjectResult(new ErrorResponse(ex.Message));
        }
    }

    [FunctionName("List")]
    [OpenApiOperation(operationId: "ListFiles", tags: new[] { ApiConstants.FilesSectionName })]
    [OpenApiSecurity(ApiConstants.BasicAuthSchemeId, SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Basic)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ApiConstants.JsonContentType, bodyType: typeof(IList<BlobItem>), Description = "Get list of files for current user")]
    public async Task<ActionResult> ListFiles(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiConstants.FilesSectionName)] HttpRequest req)
    {
        var authResult = await Authorize(req);

        if (authResult.Status != AuthResultStatus.Success)
        {
            return new UnauthorizedResult();
        }

        try
        {
            var result = await _fileService.ListFiles(authResult.UserID);

            if (result.Status == Services.Models.ResultStatus.Failed)
            {
                return new NotFoundObjectResult(new ProblemDetails
                {
                    Title = "Not found",
                    Detail = result.ErrorMessage,
                    Status = 404
                });
            }

            return new OkObjectResult(result.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could list files - {Exception}", ex);

            return new BadRequestObjectResult(new ErrorResponse(ex.Message));
        }
    }

    [FunctionName("Download")]
    [OpenApiOperation(operationId: "GetFile", tags: new[] { ApiConstants.FilesSectionName })]
    [OpenApiSecurity(ApiConstants.BasicAuthSchemeId, SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Basic)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/octet-stream", bodyType: typeof(object), Description = "Download file by ID")]
    public async Task<ActionResult> GetFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiConstants.FilesSectionName + "/{fileName}")] HttpRequest req, string fileName)
    {
        var authResult = await Authorize(req);

        if (authResult.Status != AuthResultStatus.Success)
        {
            return new UnauthorizedResult();
        }

        try
        {
            var result = await _fileService.GetFile(authResult.UserID, fileName);

            if (result.Status == Services.Models.ResultStatus.Failed)
            {
                return new NotFoundObjectResult(new ProblemDetails
                {
                    Title = "Not found",
                    Detail = result.ErrorMessage,
                    Status = 404
                });
            }

            return new PhysicalFileResult(result.Result.TemporaryPath, result.Result.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not download file - {Exception}", ex);

            return new BadRequestObjectResult(new ErrorResponse(ex.Message));
        }
    }
        
    [FunctionName("DeleteFile")]
    [OpenApiOperation(operationId: "DeleteFile", tags: new[] { ApiConstants.FilesSectionName })]
    [OpenApiSecurity(ApiConstants.BasicAuthSchemeId, SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Basic)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/octet-stream", bodyType: typeof(object), Description = "Delete file by ID")]
    public async Task<ActionResult> DeleteFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = ApiConstants.FilesSectionName + "/{fileName}")] HttpRequest req, string fileName)
    {
        var authResult = await Authorize(req);

        if (authResult.Status != AuthResultStatus.Success)
        {
            return new UnauthorizedResult();
        }

        try
        {
            var result = await _fileService.DeleteFile(authResult.UserID, fileName);

            if (!result)
            {
                return new NotFoundObjectResult(new ProblemDetails
                {
                    Title = "Not found",
                    Detail = "File not found",
                    Status = 404
                });
            }

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not delete file - {Exception}", ex);

            return new BadRequestObjectResult(new ErrorResponse(ex.Message));
        }
    }

    private async Task<AuthResult> Authorize(HttpRequest request) => await _authManager.ValidateToken(request.Headers.Authorization);
}
