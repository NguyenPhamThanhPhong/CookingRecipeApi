using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Helper;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
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
        public async Task<IEnumerable<Recipe>> GetRecipesFromIds(IEnumerable<string> ids)
        {
            var filter = Builders<Recipe>.Filter.In(s => s.id, ids);
            var sort = Builders<Recipe>.Sort.Descending(s => s.likes)/*.Descending(s=>s.createdAt);*/;
            return await _recipeCollection.Find(filter).ToListAsync();
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

        public async Task<IEnumerable<Recipe>> GetRecipesSearch(GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            var searchParams = StrUtils.SplitSpecialCharacters(searchTerm);
            var filter = Builders<Recipe>.Filter.Empty;

            if(searchParams.Length>0)
            {
                var orFilter = Builders<Recipe>.Filter.Or(
                    ChainingSearchFilter(searchParams, FilterChainType.Or));
                filter = Builders<Recipe>.Filter.Or(filter, orFilter);
            }
            if(categoriesList.Count()>0)
            {
                var andFilter = Builders<Recipe>.Filter.And(
                    ChainingSearchFilter(categoriesList, FilterChainType.And));
                filter = Builders<Recipe>.Filter.And(filter, andFilter);
            }

            var sort = Builders<Recipe>.Sort.Descending(s => s.likes)/*.Descending(s=>s.createdAt);*/;
            return await _recipeCollection.Find(filter)
                .Sort(sort).Limit(10).Skip(page).ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRecipesSaved(string userId, GetRecipeSearchRequest request, int page)
        {
            var searchTerm = request.searchTerm;
            var categoriesList = request.categories;
            string pattern = @"[.,!?;'\s]+";
            var searchParams = Regex.Split(searchTerm,pattern)
                .Where(s => s.Length > 0).ToArray();
            var savedRecipeIds= await _userCollection.Find(s => s.id == userId)
                .Project(s => s.savedRecipeIds).FirstOrDefaultAsync();
            var filter = Builders<Recipe>.Filter.In(s => s.id, savedRecipeIds);
            if (searchParams.Length > 0)
            {
                var orFilter = Builders<Recipe>.Filter.Or(
                    ChainingSearchFilter(searchParams, FilterChainType.Or));
                filter = Builders<Recipe>.Filter.Or(filter, orFilter);
            }
            if (categoriesList.Count() > 0)
            {
                var andFilter = Builders<Recipe>.Filter.And(
                    ChainingSearchFilter(categoriesList, FilterChainType.And));
                filter = Builders<Recipe>.Filter.And(filter, andFilter);
            }
            return await _recipeCollection
                .Find(filter).Skip(page * 10).Limit(10).ToListAsync();
        }
        private List<FilterDefinition<Recipe>> ChainingSearchFilter(IEnumerable<string> searchParams, FilterChainType chainType)
        {
            List<FilterDefinition<Recipe>> subfilters = new List<FilterDefinition<Recipe>>();
            if (searchParams.Count() > 0)
            {
                if(chainType == FilterChainType.And)
                {
                    foreach (string str in searchParams)
                    {
                        var regex = new BsonRegularExpression(new Regex(Regex.Escape(str), RegexOptions.IgnoreCase));
                        Console.WriteLine(str);
                        Console.WriteLine(regex);
                        subfilters.Add(Builders<Recipe>.Filter.Regex(s => s.categories, regex));
                    }
                    //filter = filter & Builders<Recipe>.Filter.Or(subfilters);
                }
                else
                {
                    foreach (string str in searchParams)
                    {
                        var regex = new BsonRegularExpression(new Regex(Regex.Escape(str), RegexOptions.IgnoreCase));
                        Console.WriteLine(str);
                        Console.WriteLine(regex);
                        subfilters.Add(Builders<Recipe>.Filter.Regex(s => s.categories, regex));
                    }
                    //filter = filter & Builders<Recipe>.Filter.Or(subfilters);
                }
            }
            return subfilters;
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
