using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Helper;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Text.RegularExpressions;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IMongoCollection<Recipe> _recipeCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly AzureBlobHandler _azureBlobHandler;
        private readonly NotificationTaskProducer _notificationTaskProducer;
        private readonly IMapper _mapper;

        public RecipeService(IRecipeRepository recipeRepository, DatabaseConfigs databaseConfigs,
            AzureBlobHandler azureBlobHandler, IMapper mapper,  NotificationTaskProducer notificationTaskProducer)
        {
            _recipeRepository = recipeRepository;
            _recipeCollection = databaseConfigs.RecipeCollection;
            _userCollection = databaseConfigs.UserCollection;
            _azureBlobHandler = azureBlobHandler;
            _mapper = mapper;
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
        public async Task<IEnumerable<Recipe>> GetRecipesFromIds(IEnumerable<string> ids)
        {
            var filter = Builders<Recipe>.Filter.In(s => s.id, ids);
            var sort = Builders<Recipe>.Sort.Descending(s => s.likes)/*.Descending(s=>s.createdAt);*/;
            return await _recipeCollection.Find(filter).ToListAsync();
        }
        public async Task<Recipe?> UpdateRecipe(RecipeUpdateRequest request, string userID)
        {
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
        public async Task<bool> SaveRecipe(string userId,string recipeId,bool option)
        {
            return await _recipeRepository.SaveRecipe(userId, recipeId, option);
        }
        public async Task<bool> LikeRecipe(string userId, string recipeId,bool option)
        {
            return await _recipeRepository.LikeRecipe(userId, recipeId, option);
        }
        public async Task<IEnumerable<Recipe>> GetRecipesSearch(GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            var searchParams = StrUtils.SplitSpecialCharacters(searchTerm);
            var filter = Builders<Recipe>.Filter.Empty;
            return await _recipeRepository.GetRecipesSearch(filter, searchParams, categoriesList, page);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesSaved(string userId, GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            var searchParams = StrUtils.SplitSpecialCharacters(searchTerm);
            var savedRecipeIds = await _userCollection.Find(s => s.id == userId)
                .Project(s => s.savedRecipeIds).FirstOrDefaultAsync();
            if(!savedRecipeIds.IsNullOrEmpty())
            {
                var filter = Builders<Recipe>.Filter.In(s => s.id, savedRecipeIds);
                return await _recipeRepository.GetRecipesSearch(filter, searchParams, categoriesList, page);
            }
            return [];
        }

        public async Task<IEnumerable<Recipe>> GetRecipesLiked(string userId, GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            var searchParams = StrUtils.SplitSpecialCharacters(searchTerm);
            var likedRecipeIds = _userCollection.Find(s => s.id == userId)
                .Project(s => s.likedRecipeIds).FirstOrDefault();
            if(!likedRecipeIds.IsNullOrEmpty())
            {
                var filter = Builders<Recipe>.Filter.In(s => s.id, likedRecipeIds);
                return await _recipeRepository.GetRecipesSearch(filter, searchParams, categoriesList, page);
            }
            return [];
        }
        public async Task<IEnumerable<Recipe>> GetRecipesOwned(string userId, GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            var searchParams = StrUtils.SplitSpecialCharacters(searchTerm);
            var ownedRecipeIds = _userCollection.Find(s => s.id == userId)
                .Project(s => s.recipeIds).FirstOrDefault();
            if (!ownedRecipeIds.IsNullOrEmpty())
            {
                var filter = Builders<Recipe>.Filter.In(s => s.id, ownedRecipeIds);
                return await _recipeRepository.GetRecipesSearch(filter, searchParams, categoriesList, page);
            }
            return [];
        }

    }
    public enum RecipeNotificationType
    {
        Creation,
        Update,
        Delete
    }
    public enum FilterChainType
    {
        And,
        Or
    }
}