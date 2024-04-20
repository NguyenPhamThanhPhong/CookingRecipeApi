namespace CookingRecipeApi.RequestsResponses.Requests.CommentRequests
{
    public class CommentUpdateRequest
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string recipeId { get; set; }
        public List<IFormFile> files { get; set; }
        public List<string> keepUrl { get; set; }
        public byte rating { get; set; }
    }
}
