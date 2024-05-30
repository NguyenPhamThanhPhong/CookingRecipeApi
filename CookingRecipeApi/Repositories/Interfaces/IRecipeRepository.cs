using CookingRecipeApi.Models;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetbyRecipeIds(IEnumerable<string> recipeIds);
        Task<Recipe?> GetbyRecipeId(string RecipeId);
        Task<Recipe?> CreateRecipe(Recipe Recipe);
        Task<Recipe?> UpdateRecipe(Recipe Recipe, string userID);
        Task<Recipe?> DeleteRecipe(string id, string userID);
        Task<bool> SaveRecipe(string userId, string recipeId, bool option);
        Task<bool> LikeRecipe(string userId, string recipeId, bool option);
        Task<IEnumerable<Recipe>> GetRecipesSearch(
                    FilterDefinition<Recipe> preConditionFilter, string[] searchParams, 
                    List<string> categoriesList, int page);
    }
}
