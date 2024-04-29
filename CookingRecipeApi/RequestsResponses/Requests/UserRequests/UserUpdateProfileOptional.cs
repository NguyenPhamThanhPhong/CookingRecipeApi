namespace CookingRecipeApi.RequestsResponses.Requests.UserRequests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class UserUpdateProfileOptional
    {
        public string? bio { get; set; }
        public List<string> categories { get; set; }
        public int hungryHeads { get; set; }
    }
}
