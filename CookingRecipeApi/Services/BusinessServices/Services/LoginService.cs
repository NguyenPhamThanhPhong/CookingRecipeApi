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
        public async Task<Tuple<string,string,User>?> LoginwithGmail(string email,string password)
        {
            var user = await _userCollection.Find(s=>s.authenticationInfo.email == email).FirstOrDefaultAsync();
            if(user.authenticationInfo.email!=email || user.authenticationInfo.password!=password)
            {
                return null;
            }
            var refreshToken = _tokenGenerator.GenerateRefreshToken();
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            return new Tuple<string,string, User>(refreshToken,accessToken,user);
        }
        public async Task<Tuple<string,User>?> LoginwithGoogle(string googleId)
        {
            var user = await _userCollection.Find(s=>s.authenticationInfo.googleId == googleId).FirstOrDefaultAsync();
            if(user.authenticationInfo.googleId!=googleId)
            {
                return null;
            }
            var token = _tokenGenerator.GenerateRefreshToken();
            return new Tuple<string, User>(token,user);
        }
        public async Task<Tuple<string, User>?> LoginwithFacebook(string facebookId)
        {
            var user = await _userCollection.Find(s => s.authenticationInfo.facebookId == facebookId).FirstOrDefaultAsync();
            if (user.authenticationInfo.facebookId != facebookId)
            {
                return null;
            }
            var token = _tokenGenerator.GenerateRefreshToken();
            return new Tuple<string, User>(token, user);
        }
        public async Task<User?> Register(RegisterRequest request)
        {
            if(request.email == null || request.password == null)
            {
                if(request.facebookId == null && request.googleId == null)
                {
                    return null;
                }
            }
            var user = _mapper.Map<User>(request);
            await _userRepository.CreateUser(user);
            return user;
        }
    }
}
