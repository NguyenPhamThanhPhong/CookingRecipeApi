using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.LoginRequests;
using CookingRecipeApi.RequestsResponses.Responses;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface ILoginService
    {
        public Task<UserLoginResponse?> LoginwithGmail(LoginRegisterRequest request);
        public Task<UserLoginResponse?> LoginwithLoginId(LoginRegisterRequest request);
        public Task<UserLoginResponse?> Register(LoginRegisterRequest request);
        public Task<User?> GetUserfromRefreshToken(string refreshToken);
        public Task<string?> GetUserPassword(string id);
    }
}
