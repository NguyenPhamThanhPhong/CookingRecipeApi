using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.RecipeRequests;
using CookingRecipeApi.Services.BusinessServices.Services;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface IRecipeService
    {
        //userId to validate the user from token
        Task<Recipe?> CreateRecipe(RecipeCreateRequest request,string userId);
        Task<Recipe?> GetRecipeById(string id);
        Task<IEnumerable<Recipe>> GetRecipes(IEnumerable<string> ids);
        Task<Recipe?> UpdateRecipe(RecipeUpdateRequest request, string userId);
        Task<bool> DeleteRecipe(string id, string userId);
        Task NotifyRecipe(string userId, string userfullName, Recipe recipe, RecipeNotificationType type);
    }
}
