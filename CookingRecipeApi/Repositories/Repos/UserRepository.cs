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

        public async Task<User?> CreateUser(User user)
        {
            try
            {
                await _userCollection.InsertOneAsync(user);
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> DeleteUser(string id)
        {
            var deletedUser = await _userCollection.FindOneAndDeleteAsync(user => user.id == id);
            return deletedUser;
        }

        public async Task<User?> GetUser(string id)
        {
            var result = await _userCollection.Find(user => user.id == id).FirstOrDefaultAsync();
            return result;
        }

        public async Task<User?> UpdateUser(User user)
        {
            var result = await _userCollection.ReplaceOneAsync(u => u.id == user.id, user);
            return (result.IsAcknowledged && result.ModifiedCount > 0)?user:null ;
        }
    }
}
