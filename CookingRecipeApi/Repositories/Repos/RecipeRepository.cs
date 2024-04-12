using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Repos
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IMongoCollection<Recipe> _recipeCollection;
        private readonly IMongoCollection<User> _userCollection;
        public RecipeRepository(DatabaseConfigs databaseConfigs)
        {
            _recipeCollection = databaseConfigs.RecipeCollection;
            _userCollection = databaseConfigs.UserCollection;
        }
        public async Task<Recipe?> CreateRecipe(Recipe recipe)
        {
            try
            {
                await _recipeCollection.InsertOneAsync(recipe);
                var userFilter = Builders<User>.Filter.Eq(u => u.id, recipe.userId);
                var userUpdate = Builders<User>.Update.Push(u => u.recipeIds, recipe.id);
                await _userCollection.UpdateOneAsync(userFilter, userUpdate);
                return recipe;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Recipe?> DeleteRecipe(string id, string userID)
        {
            var filter = Builders<Recipe>.Filter.Where(s => s.id == id && s.userId == userID);
            var result = await _recipeCollection.FindOneAndDeleteAsync(filter);
            if (result == null)
                return result;
            var userFilter = Builders<User>.Filter.Eq(u => u.id, id);
            var userUpdate = Builders<User>.Update.Pull(u => u.recipeIds, id);
            await _userCollection.UpdateOneAsync(userFilter, userUpdate);
            return result;
        }

        public async Task<Recipe?> GetbyRecipeId(string recipeId)
        {
            var recipe = await _recipeCollection.Find(p => p.id == recipeId).FirstOrDefaultAsync();
            return recipe;
        }

        public Task<IEnumerable<Recipe>> GetbyRecipeIds(IEnumerable<string> recipeIds)
        {
            var filter = Builders<Recipe>.Filter.In(p => p.id, recipeIds);
            var recipes = _recipeCollection.Find(filter).ToList();
            return Task.FromResult(recipes.AsEnumerable());
        }

        public async Task<Recipe?> UpdateRecipe(Recipe recipe, string userID)
        {
            var filter = Builders<Recipe>.Filter.Where(s => s.id == recipe.id && s.userId == userID);
            var result = await _recipeCollection.FindOneAndReplaceAsync(filter, recipe);
            return result;
        }
    }
}