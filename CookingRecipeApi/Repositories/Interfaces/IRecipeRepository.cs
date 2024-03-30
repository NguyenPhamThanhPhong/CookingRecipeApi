using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetbyPostIds(List<string> postIds);
        Task<Recipe?> GetbyPostId(string postId);
        Task<Recipe> CreatePost(Recipe post);
        Task UpdatePost(Recipe post);
        Task DeletePost(string id);
    }
}
