namespace CookingRecipeApi.RequestsResponses.CommentRequests
{
    public class CommentCreateRequest
    {
        public string recipeId { get; set; }
        public List<IFormFile> files { get; set; }
        public byte rating { get; set; }
        public string content { get; set; }
    }
}
