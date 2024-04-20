using MongoDB.Bson.Serialization.Attributes;

namespace CookingRecipeApi.Models
{
    [BsonIgnoreExtraElements]
public class CommentBatch
{
    public string id { get; set; }
    public int page { get; set; }
    public int count { get; set; }
    public string recipeId { get; set; }
    public List<Comment?> comments { get; set; }

    public CommentBatch()
    {
        this.id = string.Empty;
        page = 0;
        count = 0;
        recipeId = string.Empty;
        comments = new List<Comment?>();
    }
}
    [BsonIgnoreExtraElements]
    public class Comment
    {
        public int offSet { get; set; }
        public DateTime createdAt { get; set; }
        public byte rating { get; set; }
        public string content { get; set; }
        public List<string> attatchmentUrls { get; set; }
        public Comment()
        {
            createdAt = DateTime.Now;
            content = string.Empty;
            attatchmentUrls = new List<string>();
            rating = 0;
        }
    }
}
