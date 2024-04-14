using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CookingRecipeApi.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public User()
        {
            id = string.Empty;
            this.createdAt = DateTime.UtcNow;
            this.authenticationInfo = new AuthenticationInformation();
            this.profileInfo = new ProfileInformation();
            this.recipeIds = new List<string>();
            this.savedRecipeIds = new List<string>();
            this.followingIds = new List<string>();
            this.followerIds = new List<string>();
            this.loginTickets = new List<LoginTicket>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        public DateTime createdAt { get; set; } = DateTime.Now;
        public AuthenticationInformation authenticationInfo { get; set; }
        public ProfileInformation profileInfo { get; set; }
        public List<string> recipeIds { get; set; }
        public List<string> savedRecipeIds { get; set; }
        public List<string> followingIds { get; set; }
        public List<string> followerIds { get; set; }
        public List<LoginTicket> loginTickets { get; set; }
    }

    public class AuthenticationInformation
    {
        public string? googleId { get; set; }
        public string? facebookId { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public AuthenticationInformation()
        {
            this.googleId = string.Empty;
            this.facebookId = string.Empty;
            this.email = string.Empty;
            this.password = string.Empty;
        }
    }

    public class ProfileInformation
    {
        public string fullName { get; set; }
        public string avatarUrl { get; set; }
        public bool isVegan { get; set; }
        public string bio { get; set; }
        public List<string> categories { get; set; }
        public int hungryHeads { get; set; }
        public ProfileInformation()
        {
            this.fullName = string.Empty;
            this.avatarUrl = string.Empty;
            this.isVegan = false;
            this.bio = string.Empty;
            this.categories = new List<string>();
            this.hungryHeads = 0;
        }
    }
    public class LoginTicket
    {
        public string deviceId { get; set; }
        public string refreshToken { get; set; }
        public string deviceInfo { get; set; }
        public DateTime createTime { get; set; }
        public DateTime expireTime { get; set; }
        public LoginTicket(string refreshToken, string deviceInfo,string deviceId)
        {
            this.deviceId = deviceId;
            this.refreshToken = refreshToken;
            this.deviceInfo = deviceInfo;
            createTime = DateTime.UtcNow;
            expireTime = DateTime.UtcNow.AddDays(30);
        }
    }
}