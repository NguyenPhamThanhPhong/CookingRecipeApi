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
            this.likedRecipeIds = new List<string>();
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
        public List<string> likedRecipeIds { get; set; }
        public List<LoginTicket> loginTickets { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AuthenticationInformation
    {
        [BsonIgnoreIfNull]
        public string? loginId { get; set; }
        [BsonIgnoreIfNull]
        public string? email { get; set; }
        [BsonIgnoreIfNull]
        public string? password { get; set; }
        [BsonIgnoreIfNull]
        public string? linkedAccountType { get; set; }


    }
    [BsonIgnoreExtraElements]

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
    [BsonIgnoreExtraElements]

    public class LoginTicket
    {
        public string deviceId { get; set; }
        public string refreshToken { get; set; }
        public string deviceInfo { get; set; }
        public DateTime createTime { get; set; }
        public DateTime expireTime { get; set; }
        public override bool Equals(object? obj)
        {
            if(obj is LoginTicket ticket)
            {
                return ticket.deviceId == deviceId ;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return deviceId.GetHashCode();
        }
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