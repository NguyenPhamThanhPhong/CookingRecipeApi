using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.RecipeRequests;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using MongoDB.Driver;
using System.Linq;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IMongoCollection<Recipe> _recipeCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly ClientConstants _clientConstants;
        private readonly AzureBlobHandler _azureBlobHandler;
        private readonly NotificationTaskProducer _notificationTaskProducer;
        private readonly IMapper _mapper;

        public RecipeService(IRecipeRepository recipeRepository, DatabaseConfigs databaseConfigs,
            AzureBlobHandler azureBlobHandler, IMapper mapper, 
            ClientConstants clientConstants, NotificationTaskProducer notificationTaskProducer)
        {
            _recipeRepository = recipeRepository;
            _recipeCollection = databaseConfigs.RecipeCollection;
            _userCollection = databaseConfigs.UserCollection;
            _azureBlobHandler = azureBlobHandler;
            _mapper = mapper;
            _clientConstants = clientConstants;
            _notificationTaskProducer = notificationTaskProducer;
        }
        public async Task<Recipe?> CreateRecipe(RecipeCreateRequest request, string userID)
        {
            Recipe recipe = _mapper.Map<Recipe>(request);
            recipe.userId = userID;
            if (request.files!=null && request.files.Count>0)
            {
                var totalFilesSize = request.files.Sum(f => f.Length);
                //greater than 16MB or greater than 20 files
                if (request.files.Count > 3 || totalFilesSize > 16 * 1024 * 1024)
                    return null;
                var attatchmentUrls = await _azureBlobHandler.UploadMultipleBlobs(request.files);
                recipe.attachmentUrls = attatchmentUrls;
            }
            var result = await _recipeRepository.CreateRecipe(recipe);
            return result;
        }
        public Task<Recipe?> GetRecipeById(string id)
        {
            return _recipeRepository.GetbyRecipeId(id);
        }
        public Task<IEnumerable<Recipe>> GetRecipes(IEnumerable<string> ids)
        {
            return _recipeRepository.GetbyRecipeIds(ids);
        }
        public async Task<Recipe?> UpdateRecipe(RecipeUpdateRequest request, string userID)
        {
            //Chỉnh lại Recipe repository để có filter AND(userId, recipeId) 
            //để tránh trường hợp người dùng khác sửa recipe của người khác
            // b1. kiểm tra token match với userId trong recipe
            // b2. query thông qua cái AND ở trên
            var recipe = _mapper.Map<Recipe>(request);
            if (request.files != null && request.files.Count()>0)
            {
                var totalFilesSize = request.files.Sum(f => f.Length);
                //greater than 16MB or greater than 10 files
                if (request.files.Count > 3 || totalFilesSize > 16 * 1024 * 1024)
                {
                    return null;
                }
                var attatchmentUrls = await _azureBlobHandler.UploadMultipleBlobs(request.files);
                if (attatchmentUrls != null && recipe.attachmentUrls != null)
                    recipe.attachmentUrls.Concat(attatchmentUrls);
                else
                    recipe.attachmentUrls = attatchmentUrls ?? new List<string>();
            }
            var preUpdateRecipe = await _recipeRepository.UpdateRecipe(recipe, userID);
            if(preUpdateRecipe==null)
                return null;
            // intersect 2 list
            var deletedUrls = preUpdateRecipe.attachmentUrls.Except(recipe.attachmentUrls).ToList();
            if (deletedUrls.Count > 0)
                await _azureBlobHandler.DeleteMultipleBlobs(deletedUrls);
            return recipe;
        }
        public async Task<bool> DeleteRecipe(string id, string userID)
        {
            var recipe = await _recipeRepository.DeleteRecipe(id, userID);
            if(recipe==null)
                return false;
            if(recipe.attachmentUrls!=null && recipe.attachmentUrls.Count>0)
                await _azureBlobHandler.DeleteMultipleBlobs(recipe.attachmentUrls);
            return true;
        }
        public Task NotifyRecipe(string userId, string userfullName, Recipe recipe, RecipeNotificationType type)
        {
            var redirectPath = $"/recipe/{recipe.id}";
            var title = type==RecipeNotificationType.Creation 
                ? $"You 've got a new recipe from {userfullName}"
                : $"{userfullName} has been updated recipe {recipe.title} ";
            var message = $"{userfullName} has created a new recipe {recipe.title}";

            _notificationTaskProducer.EnqueueUserPublisherId(userId,message:message,title:title,path:redirectPath);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Recipe>> GetRecipesFromLikes()
        {
            var sort = Builders<Recipe>.Sort.Descending(s => s.likes);
            var recipes = await _recipeCollection.Find(s => true).Sort(sort).Limit(10).ToListAsync();
            return recipes;
        }
        public  Task<bool> SaveRecipe(string userId,string recipeId)
        {
            var update = Builders<User>.Update.Push(s => s.savedRecipeIds, recipeId);
            return _userCollection.UpdateOneAsync(s => s.id == userId, update)
                .ContinueWith(s => s.Result.ModifiedCount > 0);
        }
    }
    public enum RecipeNotificationType
    {
        Creation,
        Update,
        Delete
    }
}
