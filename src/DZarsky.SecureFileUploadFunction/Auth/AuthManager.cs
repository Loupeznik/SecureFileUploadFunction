using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using DZarsky.SecureFileUploadFunction.Infrastructure.Security;

namespace DZarsky.SecureFileUploadFunction.Auth
{
    public class AuthManager
    {
        private readonly CosmosClient _db;
        private readonly PasswordValidator _validator;

        private const string _databaseID = "SecureFileUploadFunction";
        private const string _containerID = "Users";

        public AuthManager(CosmosClient db, PasswordValidator validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<AuthResultStatus> ValidateCredentials(Models.User credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Login) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return AuthResultStatus.InvalidLoginOrPassword;
            }

            var container = _db.GetContainer(_databaseID, _containerID);

            var query = container
                .GetItemLinqQueryable<Models.User>()
                .Where(x => x.Login == credentials.Login)
                .ToFeedIterator();

            var user = (await query.ReadNextAsync()).FirstOrDefault();

            if (user == null || !_validator.ValidatePassword(credentials.Password, user.Password))
            {
                return AuthResultStatus.InvalidLoginOrPassword;
            }

            if (!user.IsActive)
            {
                return AuthResultStatus.UserInactive;
            }

            return AuthResultStatus.Success;
        }

        public static Models.User? ParseToken(string header)
        {
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Basic"))
            {
                var base64Credentials = header["Basic ".Length..].Trim();

                var encoding = Encoding.GetEncoding("iso-8859-1");
                var credentials = encoding.GetString(Convert.FromBase64String(base64Credentials));

                int seperatorIndex = credentials.IndexOf(':');

                return new Models.User
                {
                    Login = credentials[..seperatorIndex],
                    Password = credentials[(seperatorIndex + 1)..]
                };
            }
            else
            {
                return null;
            }
        }
    }
}
