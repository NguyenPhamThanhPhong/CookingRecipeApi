using CookingRecipeApi.Models;

namespace CookingRecipeApi.RequestsResponses.Responses
{
    public class UserLoginResponse
    {
        public string refreshToken { get; set; }
        public string accessToken { get; set; }
        public User user { get; set; }
        public UserLoginResponse(string refreshToken, string accessToken, User user)
        {
            this.refreshToken = refreshToken;
            this.accessToken = accessToken;
            this.user = user;
        }

    }
}
