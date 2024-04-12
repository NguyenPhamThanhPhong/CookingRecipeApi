using MongoDB.Bson.Serialization.Attributes;
using System.Text;
using System.Text.Json;
namespace CookingRecipeApi.Models
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string id { get; set; }
        public string userId { get; set; }
        public string title { get; set; }
        public string instruction { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public uint representIndex { get; set; }
        public List<string> attachmentUrls { get; set; }
        public List<string> Categories { get; set; }
        public int likes { get; set; }
        public List<string> commentBatchIds { get; set; }
        public TimeSpan cookTime { get; set; }
        public Dictionary<string, string> ingredients { get; set; }
        public bool isPublished { get; set; }

        public Recipe()
        {
            id = string.Empty;
            title = string.Empty;
            likes = 0;
            userId = string.Empty;
            instruction = string.Empty;
            isPublished = false;
            createdAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
            attachmentUrls = new List<string>();
            commentBatchIds = new List<string>();
            ingredients = new Dictionary<string, string>();
        }
    }
}
