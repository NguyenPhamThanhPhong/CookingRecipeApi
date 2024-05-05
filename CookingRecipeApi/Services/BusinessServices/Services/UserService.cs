using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;
using CookingRecipeApi.RequestsResponses.Responses;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;
        private readonly ProjectionDefinition<User, ProfileInformation> _profileProjection;
        private readonly AzureBlobHandler _azureBlobHandler;

        public UserService(IUserRepository userRepository, DatabaseConfigs databaseConfigs, IMapper mapper, AzureBlobHandler azureBlobHandler)
        {
            _userRepository = userRepository;
            _userCollection = databaseConfigs.UserCollection;
            _mapper = mapper;
            _profileProjection = Builders<User>.Projection.Expression(s => s.profileInfo);
            _azureBlobHandler = azureBlobHandler;
        }

        public async Task<bool> DeleteUser(string id)
        {
            var result = await _userRepository.DeleteUser(id);
            if(result==null)
                return false;
            await _azureBlobHandler.DeleteBlob(result.profileInfo.avatarUrl);
            return true;
        }

        public async Task<UserProfileResponse> getProfilebyId(string id)
        {
            var filter = Builders<User>.Filter.Where(s => s.id == id);
            var projection = 
                Builders<User>.Projection.Expression(s => new UserProfileResponse
                {
                    id = s.id,
                    createdAt = s.createdAt,
                    profileInfo = s.profileInfo,
                    recipeIds = s.recipeIds,
                    followingCount = s.followingIds.Count,
                    followerCount = s.followerIds.Count
                });
            var profile = await _userCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            return profile;
        }

        public async Task<IEnumerable<UserProfileResponse>> getProfileSearch(string search,int skip)
        {
            string cleanSearch = Regex.Replace(search, "[^a-zA-Z0-9]", "");
            var regexPattern = new BsonRegularExpression(Regex.Escape(cleanSearch), "i");
            var filter = Builders<User>.Filter.Regex(s => s.profileInfo.fullName, regexPattern);
            var projection = Builders<User>.Projection.Expression(s => new UserProfileResponse
            {
                id = s.id,
                createdAt = s.createdAt,
                profileInfo = s.profileInfo,
                recipeIds = s.recipeIds,
                followingCount = s.followingIds.Count,
                followerCount = s.followerIds.Count
            });
            var profiles = await _userCollection.Find(filter).Skip(skip).Limit(20).Project(projection).ToListAsync();
            return profiles;
        }

        public async Task<User?> getSelf(string id)
        {
            return await _userCollection.Find(s=>s.id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserProfileResponse>> GetUserFromFollowRank()
        {
            var sort = Builders<User>.Sort.Descending(s => s.followerIds);
            var users = await _userCollection.Find(s => true).Sort(sort).Limit(10).ToListAsync();
            List<UserProfileResponse> response = _mapper.Map<List<UserProfileResponse>>(users);
            return response;
        }

        public async Task<bool> UpdateFollow(string id, string followId,bool option)
        {
            var selfFilter = Builders<User>.Filter.Where(s => s.id == id);
            var targetFilter = Builders<User>.Filter.Where(s => s.id == followId);
            UpdateDefinition<User> updateTarget;
            UpdateDefinition<User> updateSelf;
            if(option)
            {
                updateSelf = Builders<User>.Update.Push(s => s.followingIds, id);
                updateTarget = Builders<User>.Update.Push(s => s.followerIds, id);
                var updateResults = await Task.WhenAll(_userCollection.UpdateOneAsync(selfFilter, updateSelf),
                                       _userCollection.UpdateOneAsync(targetFilter, updateTarget));
                return updateResults.All(s => s.IsAcknowledged && s.ModifiedCount > 0);
            }
            else
            {
                updateSelf = Builders<User>.Update.Pull(s => s.followingIds, id);
                updateTarget = Builders<User>.Update.Pull(s => s.followerIds, id);
                var updateResults = await Task.WhenAll(_userCollection.UpdateOneAsync(selfFilter, Builders<User>.Update.Pull(s => s.followingIds, id)),
                                                      _userCollection.UpdateOneAsync(targetFilter, Builders<User>.Update.Pull(s => s.followerIds, id)));
                return updateResults.All(s => s.IsAcknowledged && s.ModifiedCount > 0);
            }
        }

        public async Task<bool> UpdatePassword(string id, string password)
        {
            var filter = Builders<User>.Filter.Where(s => s.id == id);
            var update = Builders<User>.Update.Set(s => s.authenticationInfo.password, password);
            var result = await _userCollection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<ProfileInformation> UpdateProfileBasicbyId(UserUpdateRequest request, string userId)
        {
            var profile = _mapper.Map<ProfileInformation>(request);
            if(request.avatarImg != null)
            {
                var avatarUrl = await _azureBlobHandler.UploadSingleBlob(request.avatarImg);
                profile.avatarUrl = avatarUrl??"";
            }
            var filter = Builders<User>.Filter.Where(s => s.id == userId);
            var update = Builders<User>.Update.Set(s => s.profileInfo, profile);
            var user = await _userCollection.FindOneAndUpdateAsync(filter, update);
            //remove old avatar since new avatar is set
            if (request.avatarImg!= null)
                await _azureBlobHandler.DeleteBlob(user.profileInfo.avatarUrl);
            return profile;
        }

        public Task<ProfileInformation> UpdateProfileOptionalbyId(UserUpdateProfileOptional request, string userId)
        {
            var update = Builders<User>.Update
                .Set(s => s.profileInfo.bio, request.bio)
                .Set(s => s.profileInfo.categories, request.categories)
                .Set(s => s.profileInfo.hungryHeads, request.hungryHeads);
            _userCollection.UpdateOne(s => s.id == userId, update);
            return Task.FromResult(_mapper.Map<ProfileInformation>(request));
        }
    }
}