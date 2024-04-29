namespace CookingRecipeApi.RequestsResponses.Requests.LoginRequests
{
    public class LoginWithLoginIdRequest : LoginRegisterRequestBase
    {
        public string? loginId { get; set; } = string.Empty;
        //public string? linkedAccountType { get; set; }
    }
}
