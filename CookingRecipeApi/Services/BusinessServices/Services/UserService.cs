using AutoMapper;
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, DatabaseConfigs databaseConfigs, IMapper mapper)
        {
            _userRepository = userRepository;
            _userCollection = databaseConfigs.UserCollection;
            _mapper = mapper;
        }
    }
}