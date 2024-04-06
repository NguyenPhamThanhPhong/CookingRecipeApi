using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUser(string id);
        Task<User?> CreateUser(User user);
        Task<User?> UpdateUser(User user);
        Task<bool> DeleteUser(string id);
    }
}
