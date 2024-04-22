
using CookingRecipeApi.Models;

namespace CookingRecipeApi.RequestsResponses.Responses
{
    public class UserProfileResponse
    {
        public string id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public List<string> recipeIds { get; set; }
        public List<string> followingIds { get; set; }
        public List<string> followerIds { get; set; }
        public ProfileInformation profileInfo { get; set; }

        public UserProfileResponse()
        {
            id = string.Empty;
            this.createdAt = DateTime.UtcNow;
            this.profileInfo = new ProfileInformation();
            this.recipeIds = new List<string>();
            this.followingIds = new List<string>();
            this.followerIds = new List<string>();
        }

    }
}
