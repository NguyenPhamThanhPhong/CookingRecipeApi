using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.RequestsResponses.LoginRequests;
using CookingRecipeApi.Services.AuthenticationServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using MongoDB.Driver;

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
        private Task<string> StoreRefreshToken(string userId,string deviceId,string deviceInfo)
        {
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(s => s.id, userId),
                Builders<User>.Filter.ElemMatch(s => s.loginTickets, ticket => ticket.deviceId == deviceId));

            var loginTicket = new LoginTicket(refreshToken,deviceId,deviceInfo);
            var update = Builders<User>.Update.Combine(
                Builders<User>.Update.SetOnInsert(s => s.loginTickets[-1], 
                new LoginTicket(refreshToken, deviceId, deviceInfo)));
            return _userCollection.UpdateOneAsync(filter, update)
                .ContinueWith(returnValue => refreshToken);
        }
        public async Task<Tuple<string,string,User>?> LoginwithGmail(string email,string password)
        {
            var user = await _userCollection.Find(s=>s.authenticationInfo.email == email).FirstOrDefaultAsync();
            if(user==null || user.authenticationInfo.email!=email 
                || user.authenticationInfo.password!=password)
            {
                return null;
            }
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new Tuple<string,string, User>(refreshToken,accessToken,user);
        }
        public async Task<Tuple<string, string, User>?> LoginwithGoogle(string googleId)
        {
            var user = await _userCollection.Find(s=>s.authenticationInfo.googleId == googleId).FirstOrDefaultAsync();
            if(user.authenticationInfo.googleId!=googleId)
            {
                return null;
            }
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new Tuple<string, string, User>(refreshToken, accessToken, user);
        }
        public async Task<Tuple<string,string, User>?> LoginwithFacebook(string facebookId)
        {
            var user = await _userCollection.Find(s => s.authenticationInfo.facebookId == facebookId).FirstOrDefaultAsync();
            if (user.authenticationInfo.facebookId != facebookId)
            {
                return null;
            }
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new Tuple<string, string, User>(refreshToken, accessToken, user);
        }
        public async Task<Tuple<string, string, User>?> Register(RegisterRequest request)
        {
            if(request.email == null || request.password == null)
            {
                if(request.facebookId == null && request.googleId == null)
                {
                    return null;
                }
            }
            var user = _mapper.Map<User>(request);
            user = await _userRepository.CreateUser(user);
            if (user == null)
                return null;
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new Tuple<string, string, User>(refreshToken, accessToken, user);
        }

        public async Task<User?> GetUserfromRefreshToken(string refreshToken)
        {
            var filter = Builders<User>.Filter.ElemMatch(
                s => s.loginTickets,
                ticket=>ticket.refreshToken==refreshToken);
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
