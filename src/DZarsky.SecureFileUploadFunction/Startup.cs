using System;
using DZarsky.SecureFileUploadFunction.Auth;
using DZarsky.SecureFileUploadFunction.Infrastructure.Security;
using DZarsky.SecureFileUploadFunction.Services;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DZarsky.SecureFileUploadFunction.Startup))]
namespace DZarsky.SecureFileUploadFunction
{
    public class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Startup>(true)
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((s) =>
            {
                var endpoint = configuration.GetSection("CosmosDB").GetValue<string>("Endpoint");

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new ArgumentException("CosmosDB endpoint was not set");
                }

                string authKey = configuration.GetSection("CosmosDB").GetValue<string>("AuthorizationKey");

                if (string.IsNullOrWhiteSpace(authKey))
                {
                    throw new ArgumentException("CosmosDB authorization key was not set");
                }

                var configurationBuilder = new CosmosClientBuilder(endpoint, authKey);

                return configurationBuilder
                        .Build();
            });

            builder.Services.AddScoped<AuthManager>();
            builder.Services.AddScoped<PasswordValidator>();
            builder.Services.AddScoped<FileService>();
        }
    }
}
