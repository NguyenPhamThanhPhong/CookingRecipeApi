using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Repos
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IMongoCollection<Recipe> _postCollection;
        private readonly IMongoCollection<User> _userCollection;
        public RecipeRepository(DatabaseConfigs databaseConfigs)
        {
            _postCollection = databaseConfigs.RecipeCollection;
            _userCollection = databaseConfigs.UserCollection;
        }
        public async Task<Recipe> CreatePost(Recipe post)
        {
            await _postCollection.InsertOneAsync(post);
            var userFilter = Builders<User>.Filter.Eq(u => u.id, post.userId);
            var userUpdate = Builders<User>.Update.Push(u => u.recipeIds, post.id);
            await _userCollection.UpdateOneAsync(userFilter, userUpdate);
            return post;
        }

        public async Task DeletePost(string id)
        {
            await _postCollection.FindOneAndDeleteAsync(p => p.id == id);
            var userFilter = Builders<User>.Filter.Eq(u => u.id, id);
            var userUpdate = Builders<User>.Update.Pull(u => u.recipeIds, id);
            await _userCollection.UpdateOneAsync(userFilter, userUpdate);
        }

        public async Task<Recipe?> GetbyPostId(string postId)
        {
            var post = await _postCollection.Find(p => p.id == postId).FirstOrDefaultAsync();
            return post;
        }

        public Task<IEnumerable<Recipe>> GetbyPostIds(List<string> postIds)
        {
            var posts = _postCollection.Find(p => postIds.Contains(p.id)).ToList();
            return Task.FromResult(posts.AsEnumerable());
        }

        public async Task UpdatePost(Recipe post)
        {
           await _postCollection.ReplaceOneAsync(p => p.id == post.id, post);
        }
    }
}