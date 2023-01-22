using DZarsky.SecureFileUploadFunction.Common;
using DZarsky.SecureFileUploadFunction.Infrastructure.Security;
using DZarsky.SecureFileUploadFunction.Models;
using DZarsky.SecureFileUploadFunction.Services.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DZarsky.SecureFileUploadFunction.Services
{
    public sealed class UserService
    {
        private readonly CosmosClient _db;
        private readonly PasswordHasher _passwordHasher;

        public UserService(CosmosClient db, PasswordHasher hasher)
        {
            _db = db;
            _passwordHasher = hasher;
        }

        public async Task<CreateUserResult> CreateUser(UserDto credentials)
        {
            var container = _db.GetContainer(CosmosConstants.DatabaseID, CosmosConstants.ContainerID);

            var userByLogin = container
                .GetItemLinqQueryable<SecureFileUploadFunction.Models.User>()
                .Where(x => x.Login == credentials.Login)
                .ToFeedIterator();

            if ((await userByLogin.ReadNextAsync()).Any())
            {
                return new CreateUserResult(ResultStatus.AlreadyExists);
            }

            var hashedPassword = _passwordHasher.HashPassword(credentials.Password);

            var user = new SecureFileUploadFunction.Models.User
            {
                Id = Guid.NewGuid().ToString(),
                Login = credentials.Login,
                Password = hashedPassword,
                DateCreated = DateTime.Now
            };

            var response = await container.CreateItemAsync(user);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                return new CreateUserResult(ResultStatus.Failed);
            }

            response.Resource.Password = null;

            return new CreateUserResult(ResultStatus.Success, response.Resource);
        }
    }
}
