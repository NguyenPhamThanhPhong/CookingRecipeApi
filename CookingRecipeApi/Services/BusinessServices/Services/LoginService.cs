using AutoMapper;
using Azure.Core;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.Requests.LoginRequests;
using CookingRecipeApi.RequestsResponses.Responses;
using CookingRecipeApi.Services.AuthenticationServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMongoCollection<User> _userCollection;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;

        public LoginService(IUserRepository userRepository, DatabaseConfigs databaseConfigs, TokenGenerator tokenGenerator, IMapper mapper)
        {
            _userRepository = userRepository;
            _userCollection = databaseConfigs.UserCollection;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
        }
        private async Task<User?> _storeRefreshToken(
            FilterDefinition<User> filter, LoginTicket loginTicket)
        {
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var updatePull = Builders<User>.Update
                    .PullFilter(s => s.loginTickets,
                    Builders<LoginTicket>.Filter.Eq(x => x.deviceId, loginTicket.deviceId));
            var updatePush = Builders<User>.Update.AddToSet(s => s.loginTickets, loginTicket);
            var findOptions = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };
            await _userCollection.FindOneAndUpdateAsync(filter, updatePull, findOptions);
            var result = await _userCollection.FindOneAndUpdateAsync(filter, updatePush, findOptions);
            return result;
        }
        private LoginTicket _generateLoginTicket(LoginRegisterRequestBase request)
        {
            LoginTicket loginTicket = _mapper.Map<LoginTicket>(request);
            loginTicket.refreshToken = _tokenGenerator.GenerateRefreshToken();
            return loginTicket;
        }
        public async Task<UserLoginResponse?> LoginwithGmail(LoginWithEmailRequest request)
        {
            var filter = Builders<User>.Filter
                .Where(s=>s.authenticationInfo.email == request.email 
                && s.authenticationInfo.password==request.password);
            LoginTicket loginTicket = _generateLoginTicket(request);
            var user = await _storeRefreshToken(filter, loginTicket);
            if (user == null )
                return null;
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new UserLoginResponse(loginTicket.refreshToken, accessToken, user);
        }
        public async Task<UserLoginResponse?> LoginwithLoginId(LoginWithLoginIdRequest request)
        {
            var filter = Builders<User>.Filter.Eq(s => s.authenticationInfo.loginId, request.loginId);
            LoginTicket loginTicket = _generateLoginTicket(request);
            var user = await _storeRefreshToken(filter, loginTicket);
            if (user == null)
                return null;
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new UserLoginResponse(loginTicket.refreshToken, accessToken, user);
        }
        public async Task<UserLoginResponse?> RegisterWithEmail(RegisterWithEmailRequest request)
        {
            if (request.email == null || request.password == null)
                return null;
            var user = _mapper.Map<User>(request);
            user.loginTickets.Add(_generateLoginTicket(request));
            user = await _userRepository.CreateUser(user);
            if (user == null)
                return null;
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new UserLoginResponse(refreshToken, accessToken, user);
        }
        public async Task<UserLoginResponse?> RegisterWithLoginId(RegisterWithLoginIdRequest request)
        {
            if (request.loginId == null)
                return null;
            var user = _mapper.Map<User>(request);
            user.loginTickets.Add(_generateLoginTicket(request));
            user = await _userRepository.CreateUser(user);
            if (user == null)
                return null;
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new UserLoginResponse(refreshToken, accessToken, user);
        }

        public async Task<User?> GetUserfromRefreshToken(string refreshToken)
        {
            var filter = Builders<User>.Filter.ElemMatch(
                s => s.loginTickets,
                ticket => ticket.refreshToken == refreshToken);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }
        public async Task<string?> GetUserPassword(string userId)
        {
            var projection = Builders<User>.Projection.Include(x => x.authenticationInfo.password);
            var password = await _userCollection.Find(s => s.id == userId).Project<string>(projection).FirstOrDefaultAsync();
            return password;
        }


    }
}
