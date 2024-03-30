using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Repos
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(DatabaseConfigs databaseConfigs)
        {
            _userCollection = databaseConfigs.UserCollection;
        }

        public async Task<User> CreateUser(User user)
        {
            await _userCollection.InsertOneAsync(user);
            return user;
        }

        public Task DeleteUser(string id)
        {
            return _userCollection.DeleteOneAsync(user => user.id == id);
        }

        public Task<User> GetUser(string id)
        {
            return _userCollection.Find(user => user.id == id).FirstOrDefaultAsync();
        }

        public Task UpdateUser(User user)
        {
            return _userCollection.ReplaceOneAsync(u => u.id == user.id, user);
        }
    }
}
