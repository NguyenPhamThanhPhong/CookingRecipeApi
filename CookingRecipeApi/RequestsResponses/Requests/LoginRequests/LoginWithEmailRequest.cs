namespace CookingRecipeApi.RequestsResponses.Requests.LoginRequests
{
    public class LoginWithEmailRequest : LoginRegisterRequestBase
    {
        public string? email { get; set; }
        public string? password { get; set; }

    }
}
