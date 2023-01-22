using DZarsky.SecureFileUploadFunction.Infrastructure.Configuration;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DZarsky.SecureFileUploadFunction.Infrastructure.Security
{
    public sealed class PasswordHasher
    {
        private readonly IConfiguration _configuration;

        public PasswordHasher(IConfiguration configuration) => _configuration = configuration;

        public Argon2Config GetHasherConfiguration(string password)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Create().GetBytes(salt);

            return new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 10,
                MemoryCost = 32768,
                Lanes = 5,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt,
                Secret = Encoding.UTF8.GetBytes(_configuration.GetValueFromContainer<string>("ArgonSecret")),
                HashLength = 20
            };
        }

        public string HashPassword(string password)
        {
            var config = GetHasherConfiguration(password);

            return Argon2.Hash(config);
        }
    }
}
