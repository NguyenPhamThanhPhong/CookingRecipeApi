using CookingRecipeApi.Configs;
using CookingRecipeApi.Helper;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.Services.BusinessServices.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

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
        public async Task<bool> SaveRecipe(string userId, string recipeId, bool option)
        {
            try
            {
                if (option)
                {
                    var pullUpdate = Builders<User>.Update.Pull(s => s.savedRecipeIds, recipeId);

                    var update = Builders<User>.Update.Push(s => s.savedRecipeIds, recipeId);

                    await _userCollection.UpdateOneAsync(s => s.id == userId, pullUpdate);
                    var updateResult = await _userCollection.UpdateOneAsync(s => s.id == userId, update);
                    return updateResult.ModifiedCount > 0;
                }
                else
                {
                    var update = Builders<User>.Update.Pull(s => s.savedRecipeIds, recipeId);
                    var updateResult = await _userCollection.UpdateOneAsync(s => s.id == userId, update);
                    return updateResult.ModifiedCount > 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> LikeRecipe(string userId, string recipeId, bool option)
        {
            try
            {
                if (option)
                {
                    var pullUpdate = Builders<User>.Update.Pull(s => s.likedRecipeIds, recipeId);

                    var update = Builders<User>.Update.Push(s => s.likedRecipeIds, recipeId);

                    await _userCollection.UpdateOneAsync(s => s.id == userId, pullUpdate);
                    var updateResult = await _userCollection.UpdateOneAsync(s => s.id == userId, update);
                    return updateResult.ModifiedCount > 0;
                }
                else
                {
                    var update = Builders<User>.Update.Pull(s => s.likedRecipeIds, recipeId);
                    var updateResult = await _userCollection.UpdateOneAsync(s => s.id == userId, update);
                    return updateResult.ModifiedCount > 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<IEnumerable<Recipe>> GetRecipesSearch(
            FilterDefinition<Recipe> preConditionFilter,string[] searchParams,List<string> categoriesList, int page)
        {
            if (searchParams.Length > 0)
            {
                var orFilter = Builders<Recipe>.Filter.Or(
                    ChainingSearchFilter(searchParams, FilterChainType.Or));
                preConditionFilter = Builders<Recipe>.Filter.And(preConditionFilter, orFilter);
            }
            if (categoriesList.Count() > 0)
            {
                var andFilter = Builders<Recipe>.Filter.And(
                    ChainingSearchFilter(categoriesList, FilterChainType.And));
                preConditionFilter = Builders<Recipe>.Filter.And(preConditionFilter, andFilter);
            }

            var sort = Builders<Recipe>.Sort.Descending(s => s.likes)/*.Descending(s=>s.createdAt);*/;
            return await _recipeCollection.Find(preConditionFilter)
                .Sort(sort).Skip(page * 10).Limit(10).ToListAsync();
        }
        private List<FilterDefinition<Recipe>> ChainingSearchFilter(IEnumerable<string> searchParams, FilterChainType chainType)
        {
            List<FilterDefinition<Recipe>> subfilters = new List<FilterDefinition<Recipe>>();
            if (searchParams.Count() > 0)
            {
                if (chainType == FilterChainType.And)
                {
                    foreach (string str in searchParams)
                    {
                        var regex = new BsonRegularExpression(new Regex(Regex.Escape(str), RegexOptions.IgnoreCase));
                        subfilters.Add(Builders<Recipe>.Filter.Regex(s => s.categories, regex));
                    }
                }
                else
                {
                    foreach (string str in searchParams)
                    {
                        var regex = new BsonRegularExpression(new Regex(Regex.Escape(str), RegexOptions.IgnoreCase));
                        subfilters.Add(Builders<Recipe>.Filter.Regex(s => s.title, regex));
                    }
                }
            }
            return subfilters;
        }
    }
}