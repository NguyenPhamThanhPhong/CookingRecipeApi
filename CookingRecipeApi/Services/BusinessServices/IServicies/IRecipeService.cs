using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.Services.BusinessServices.Services;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface IRecipeService
    {
        //userId to validate the user from token
        Task<Recipe?> CreateRecipe(RecipeCreateRequest request,string userId);
        Task<Recipe?> GetRecipeById(string id);
        Task<IEnumerable<Recipe>> GetRecipesSaved(string userId,int page);
        Task<IEnumerable<Recipe>> GetRecipesSearch(string searchTerm,int page);
        Task<IEnumerable<Recipe>> GetRecipesFromIds(IEnumerable<string> ids,string searchTerm,int page);
        Task<Recipe?> UpdateRecipe(RecipeUpdateRequest request, string userId);
        Task<bool> DeleteRecipe(string id, string userId);
        Task NotifyRecipe(string userId, string userfullName, Recipe recipe, RecipeNotificationType type);
        Task<IEnumerable<Recipe>> GetRecipesFromLikes();
        Task<bool> SaveRecipe(string userId, string recipeId);
    }
}
