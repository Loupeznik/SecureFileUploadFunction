using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DZarsky.SecureFileUploadFunction.Infrastructure.Security
{
    public class PasswordValidator
    {
        private readonly IConfiguration _configuration;

        public PasswordValidator(IConfiguration configuration) => _configuration = configuration;

        /// <summary>
        /// Validates the input password against a saved hashed password
        /// </summary>
        /// <param name="password">The plaintext input password</param>
        /// <param name="hashedPassword">The hashed stored password</param>
        /// <returns>Returns true if validation is successfull, otherwise returns false</returns>
        public bool ValidatePassword(string password, string hashedPassword)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Create().GetBytes(salt);

            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 10,
                MemoryCost = 32768,
                Lanes = 5,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt,
                Secret = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("ArgonSecret")),
                HashLength = 20
            };

            if (Argon2.Verify(hashedPassword, config))
            {
                return true;
            }

            return false;
        }
    }
}
