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
using DZarsky.CommonLibraries.AzureFunctions.Extensions;

namespace DZarsky.SecureFileUploadFunction.Services
{
    public sealed class FileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration) => _configuration = configuration;

        public async Task<UploadFileResult> UploadFile(IFormFile file, string userId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            var blobClient = GetBlobClient(userId, file.FileName);
            var containerExists = await GetBlobContainerClient(userId).ExistsAsync();

            if (!containerExists.Value)
            {
                await CreateContainer(userId);
            }

            if (await blobClient.ExistsAsync())
            {
                return new UploadFileResult(ResultStatus.AlreadyExists);
            }

            var result = await blobClient.UploadAsync(memoryStream);

            return new UploadFileResult(ResultStatus.Success, result.Value);
        }

        public async Task<BlobResultWrapper<AsyncPageable<BlobItem>>> ListFiles(string userId)
        {
            var blobClient = GetBlobContainerClient(userId);

            if (!await blobClient.ExistsAsync())
            {
                return new BlobResultWrapper<AsyncPageable<BlobItem>>(ResultStatus.Failed, "Container does not exist");
            }

            return new BlobResultWrapper<AsyncPageable<BlobItem>>(ResultStatus.Success, result: blobClient.GetBlobsAsync());
        }

        public async Task<BlobResultWrapper<GetFileResult>> GetFile(string userId, string fileName)
        {
            var blobClient = GetBlobClient(userId, fileName);

            if (!await blobClient.ExistsAsync())
            {
                return new BlobResultWrapper<GetFileResult>(ResultStatus.Failed, "Blob does not exist");
            }

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{Path.GetExtension(fileName)}");

            await using var stream = await blobClient.OpenReadAsync();

            await using var fileStream = File.OpenWrite(tempPath);
            await stream.CopyToAsync(fileStream);

            return new BlobResultWrapper<GetFileResult>(ResultStatus.Success, result: new GetFileResult(tempPath, MimeTypeHelper.ResolveMimeType(tempPath)));
        }
        
        public async Task<bool> DeleteFile(string userId, string fileName)
        {
            var blobClient = GetBlobClient(userId, fileName);

            return (await blobClient.DeleteIfExistsAsync()).Value;
        }

        private async Task CreateContainer(string userId) => await GetBlobContainerClient(userId).CreateAsync();

        private BlobClient GetBlobClient(string userId, string fileName) => new(
                _configuration.GetValueFromContainer<string>("UploadEndpoint"),
                userId,
                fileName);

        private BlobContainerClient GetBlobContainerClient(string userId) => new(
                _configuration.GetValueFromContainer<string>("UploadEndpoint"),
                userId);
    }
}
