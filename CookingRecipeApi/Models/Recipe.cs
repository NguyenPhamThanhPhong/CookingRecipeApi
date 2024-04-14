using MongoDB.Bson.Serialization.Attributes;
using System.Text;
using System.Text.Json;
namespace CookingRecipeApi.Models
{
    [BsonIgnoreExtraElements]
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string id { get; set; }
        public string userId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string instruction { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int representIndex { get; set; }
        public List<string> attachmentUrls { get; set; }
        public string categories { get; set; }
        public int likes { get; set; }
        public TimeSpan cookTime { get; set; }
        public Dictionary<string, string> ingredients { get; set; }
        public bool isPublished { get; set; }

        public Recipe()
        {
            id = string.Empty;
            title = string.Empty;
            likes = 0;
            representIndex = -1;
            description = string.Empty;
            categories = string.Empty;
            userId = string.Empty;
            instruction = string.Empty;
            isPublished = false;
            createdAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
            attachmentUrls = new List<string>();
            ingredients = new Dictionary<string, string>();
        }
    }
}
