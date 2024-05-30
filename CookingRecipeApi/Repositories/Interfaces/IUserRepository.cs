using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUser(string id);
        Task<User?> CreateUser(User user);
        Task<User?> UpdateUser(User user);
        Task<User?> DeleteUser(string id);
        Task<IEnumerable<User>> SearchUser(string searchParam, int page);
    }
}
