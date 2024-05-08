using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.LoginRequests;
using CookingRecipeApi.RequestsResponses.Responses;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface ILoginService
    {
        public Task<bool> VerifyLogin(string email);
        public Task<UserLoginResponse?> LoginwithGmail(LoginWithEmailRequest request);
        public Task<UserLoginResponse?> LoginwithLoginId(LoginWithLoginIdRequest request);
        public Task<UserLoginResponse?> RegisterWithEmail(RegisterWithEmailRequest request);
        public Task<UserLoginResponse?> RegisterWithLoginId(RegisterWithLoginIdRequest request);
        public Task<User?> GetUserfromRefreshToken(string refreshToken);
        public Task<string?> GetUserPassword(string id);
    }
}
