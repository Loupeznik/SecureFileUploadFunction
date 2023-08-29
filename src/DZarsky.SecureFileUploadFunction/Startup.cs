using System;
using DZarsky.CommonLibraries.AzureFunctions.Infrastructure;
using DZarsky.CommonLibraries.AzureFunctions.Models.Auth;
using DZarsky.SecureFileUploadFunction.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DZarsky.SecureFileUploadFunction.Startup))]
namespace DZarsky.SecureFileUploadFunction
{
    internal class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Startup>(true)
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddCommonFunctionServices(_configuration, AuthType.Zitadel);

            builder.Services.AddScoped<FileService>();
        }
    }
}
