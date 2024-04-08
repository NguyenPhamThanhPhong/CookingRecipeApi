using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CookingRecipeApi.Models
{
    public class NotificationBatch
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string userId { get; set; }
        public int page { get; set; } 
        public int count { get; set; }
        public List<Notification?> notifications { get; set; }
        public NotificationBatch()
        {
            id = string.Empty;
            userId = string.Empty;
            notifications = new List<Notification?>();
            count = 0;
        }
    }


    public class Notification
    {
        public int offSet { get; set; }
        public DateTime createdAt { get; set; }
        public string? imageUrl { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public bool isRead { get; set; }
        public string? redirectPath { get; set; }
        public Notification()
        {
            createdAt = DateTime.Now;
            isRead = false;
            title = string.Empty;
            content = string.Empty;
        }

    }
}
