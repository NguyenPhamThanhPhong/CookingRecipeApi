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
        Task<IEnumerable<Recipe>> GetRecipesSaved(string userId,GetRecipeSearchRequest request,int page);
        Task<IEnumerable<Recipe>> GetRecipesLiked(string userId, GetRecipeSearchRequest request, int page);
        Task<IEnumerable<Recipe>> GetRecipesOwned(string userId, GetRecipeSearchRequest request, int page);
        Task<IEnumerable<Recipe>> GetRecipesSearch(GetRecipeSearchRequest request,int page);
        Task<IEnumerable<Recipe>> GetRecipesFromIds(IEnumerable<string> recipeIds);
        Task<Recipe?> UpdateRecipe(RecipeUpdateRequest request, string userId);
        Task<bool> DeleteRecipe(string id, string userId);
        Task NotifyRecipe(string userId, string userfullName, Recipe recipe, RecipeNotificationType type);
        Task<IEnumerable<Recipe>> GetRecipesFromLikes();
        Task<bool> SaveRecipe(string userId, string recipeId,bool option);
        Task<bool> LikeRecipe(string userId, string recipeId, bool option);
    }
}
