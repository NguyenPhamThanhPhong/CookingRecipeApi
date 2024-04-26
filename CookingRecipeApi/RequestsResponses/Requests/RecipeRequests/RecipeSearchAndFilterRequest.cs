namespace CookingRecipeApi.RequestsResponses.Requests.RecipeRequests
{
    public class RecipeSearchAndFilterRequest
    {
        public string searchTerm { get; set; }
        public IEnumerable<string> categories { get; set; }
        public RecipeSearchAndFilterRequest()
        {
            searchTerm = string.Empty;
            categories = new List<string>();
        }
    }
}
