using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetbyRecipeIds(IEnumerable<string> recipeIds);
        Task<Recipe?> GetbyRecipeId(string RecipeId);
        Task<Recipe?> CreateRecipe(Recipe Recipe);
        Task<bool> UpdateRecipe(Recipe Recipe,string userID);
        Task<bool> DeleteRecipe(string id, string userID);
    }
}
