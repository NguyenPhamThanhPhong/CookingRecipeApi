
using CookingRecipeApi.Models;

namespace CookingRecipeApi.RequestsResponses.Responses
{
    public class UserProfileResponse
    {
        public string id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public List<string> recipeIds { get; set; }
        public int followingCount { get; set; }
        public int followerCount { get; set; }
        public ProfileInformation profileInfo { get; set; }

        public UserProfileResponse()
        {
            id = string.Empty;
            this.createdAt = DateTime.UtcNow;
            this.profileInfo = new ProfileInformation();
            this.recipeIds = new List<string>();
            followerCount = 0;
            followingCount = 0;
        }

    }
}
