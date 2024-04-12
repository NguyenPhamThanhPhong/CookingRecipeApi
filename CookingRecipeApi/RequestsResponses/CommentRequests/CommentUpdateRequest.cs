namespace CookingRecipeApi.RequestsResponses.CommentRequests
{
    public class CommentUpdateRequest
    {
        public string recipeId { get; set; }
        public List<IFormFile> files { get; set; }
        public List<string> keepUrl { get; set; }
        public byte rating { get; set; }
    }
}
