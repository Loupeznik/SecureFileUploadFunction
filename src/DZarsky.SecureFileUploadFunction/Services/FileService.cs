using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace DZarsky.SecureFileUploadFunction.Services
{
    public class FileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration) => _configuration = configuration;

        public async Task<Response<BlobContentInfo>> UploadFile(IFormFile file, string userID)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            var blobClient = new BlobClient(
                _configuration.GetValue<string>("UploadEndpoint"),
                userID,
                file.FileName);

            if (!await blobClient.ExistsAsync())
            {
                await CreateContainer(userID);
            }

            return await blobClient.UploadAsync(memoryStream);
        }

        public Task<AsyncPageable<BlobItem>> ListFiles(string userID)
        {
            var blobClient = new BlobContainerClient(
                _configuration.GetValue<string>("UploadEndpoint"),
                userID);

            return Task.FromResult(blobClient.GetBlobsAsync());
        }

        private async Task CreateContainer(string userID)
        {
            var containerClient = new BlobContainerClient(
                _configuration.GetValue<string>("UploadEndpoint"),
                userID);

            await containerClient.CreateAsync();
        }
    }
}
