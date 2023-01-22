using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DZarsky.SecureFileUploadFunction.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using DZarsky.SecureFileUploadFunction.Services.Helpers;
using System;

namespace DZarsky.SecureFileUploadFunction.Services
{
    public sealed class FileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration) => _configuration = configuration;

        public async Task<UploadFileResult> UploadFile(IFormFile file, string userID)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            var blobClient = GetBlobClient(userID, file.FileName);
            var containerExists = await GetBlobContainerClient(userID).ExistsAsync();

            if (!containerExists.Value)
            {
                await CreateContainer(userID);
            }

            if (await blobClient.ExistsAsync())
            {
                return new UploadFileResult(ResultStatus.AlreadyExists);
            }

            var result = await blobClient.UploadAsync(memoryStream);

            return new UploadFileResult(ResultStatus.Success, result.Value);
        }

        public async Task<BlobResultWrapper<AsyncPageable<BlobItem>>> ListFiles(string userID)
        {
            var blobClient = GetBlobContainerClient(userID);

            if (!await blobClient.ExistsAsync())
            {
                return new BlobResultWrapper<AsyncPageable<BlobItem>>(ResultStatus.Failed, "Container does not exist");
            }

            return new BlobResultWrapper<AsyncPageable<BlobItem>>(ResultStatus.Success, result: blobClient.GetBlobsAsync());
        }

        public async Task<BlobResultWrapper<GetFileResult>> GetFile(string userID, string fileName)
        {
            var blobClient = GetBlobClient(userID, fileName);

            if (!await blobClient.ExistsAsync())
            {
                return new BlobResultWrapper<GetFileResult>(ResultStatus.Failed, "Blob does not exist");
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{Path.GetExtension(fileName)}");

            using var stream = await blobClient.OpenReadAsync();

            using var fileStream = File.OpenWrite(tempPath);
            await stream.CopyToAsync(fileStream);

            return new BlobResultWrapper<GetFileResult>(ResultStatus.Success, result: new GetFileResult(tempPath, MimeTypeHelper.ResolveMimeType(tempPath)));
        }

        private async Task CreateContainer(string userID) => await GetBlobContainerClient(userID).CreateAsync();

        private BlobClient GetBlobClient(string userID, string fileName) => new(
                _configuration.GetValue<string>("UploadEndpoint"),
                userID,
                fileName);

        private BlobContainerClient GetBlobContainerClient(string userID) => new(
                _configuration.GetValue<string>("UploadEndpoint"),
                userID);
    }
}
