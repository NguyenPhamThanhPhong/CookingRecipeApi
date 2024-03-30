using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUser(string id);
        Task<User> CreateUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(string id);
    }
}
