namespace CookingRecipeApi.RequestsResponses.LoginRequests
{
    public class LoginRequest
    {
        public string deviceInfo { get; set; } = string.Empty;
        public string deviceId { get; set; } = string.Empty;
        public string? googleId { get; set; }
        public string? facebookId { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }


    }
}
