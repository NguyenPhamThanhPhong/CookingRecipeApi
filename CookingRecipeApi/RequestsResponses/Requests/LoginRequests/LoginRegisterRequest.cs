namespace CookingRecipeApi.RequestsResponses.Requests.LoginRequests
{
    public class LoginRegisterRequest
    {
        public string? deviceInfo { get; set; } = string.Empty;
        public string? deviceId { get; set; } = string.Empty;
        public string? loginId { get; set; } = string.Empty;
        public string? email { get; set; }
        public string? password { get; set; }


    }
}
