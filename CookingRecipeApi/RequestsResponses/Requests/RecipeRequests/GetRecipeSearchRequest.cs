namespace CookingRecipeApi.RequestsResponses.Requests.RecipeRequests
{
    public class GetRecipeSearchRequest
    {
        public string searchTerm { get; set; }
        public List<string> categories { get; set; }
        public GetRecipeSearchRequest()
        {
            searchTerm = string.Empty;
            categories = new List<string>();
        }

    }
}