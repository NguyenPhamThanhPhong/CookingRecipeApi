using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.LoginRequests;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface ILoginService
    {
        public Task<Tuple<string,string,User>?> LoginwithGmail(string email,string password);
        public Task<Tuple<string, string, User>?> LoginwithGoogle(string googleId);
        public Task<Tuple<string, string, User>?> LoginwithFacebook(string facebookId);
        public Task<Tuple<string, string, User>?> Register(RegisterRequest request);
        public Task<User?> GetUserfromRefreshToken(string refreshToken);
        public Task<string?> GetUserPassword(string id);
    }
}
