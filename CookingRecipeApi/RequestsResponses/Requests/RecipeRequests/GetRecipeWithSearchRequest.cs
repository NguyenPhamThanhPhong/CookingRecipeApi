namespace CookingRecipeApi.RequestsResponses.Requests.RecipeRequests
{
    public class GetRecipeWithSearchRequest
    {
        public IEnumerable<string> recipeIds { get; set; }
        public string searchTerm { get; set; }
    }
}
