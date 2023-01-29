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
        private readonly PasswordValidator _passwordValidator;

        public UserService(CosmosClient db, PasswordHasher hasher, PasswordValidator validator)
        {
            _db = db;
            _passwordHasher = hasher;
            _passwordValidator = validator;
        }

        public async Task<UserInfoResult> CreateUser(UserDto credentials)
        {
            var container = _db.GetContainer(CosmosConstants.DatabaseID, CosmosConstants.ContainerID);

            var userByLogin = container
                .GetItemLinqQueryable<SecureFileUploadFunction.Models.User>()
                .Where(x => x.Login == credentials.Login)
                .ToFeedIterator();

            if ((await userByLogin.ReadNextAsync()).Any())
            {
                return new UserInfoResult(ResultStatus.AlreadyExists);
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
                return new UserInfoResult(ResultStatus.Failed);
            }

            response.Resource.Password = null;

            return new UserInfoResult(ResultStatus.Success, response.Resource);
        }

        public async Task<UserInfoResult> GetInfo(UserDto credentials)
        {
            var container = _db.GetContainer(CosmosConstants.DatabaseID, CosmosConstants.ContainerID);

            var userByLogin = container
                .GetItemLinqQueryable<SecureFileUploadFunction.Models.User>()
                .Where(x => x.Login == credentials.Login)
                .ToFeedIterator();

            var user = (await userByLogin.ReadNextAsync()).FirstOrDefault();

            if (user == null || !_passwordValidator.ValidatePassword(credentials.Password, user.Password))
            {
                return new UserInfoResult(ResultStatus.NotFound);
            }

            user.Password = null;

            return new UserInfoResult(ResultStatus.Success, user);
        }
    }
}
