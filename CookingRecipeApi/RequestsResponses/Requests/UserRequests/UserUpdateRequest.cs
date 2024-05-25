using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.RequestsResponses.Requests.UserRequests
{
    public class UserUpdateRequest
    {
        //#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string fullName { get; set; }
        public string? avatarUrl { get; set; }
        public string? googleLink { get; set; }
        public string? facebookLink { get; set; }
        public IFormFile? avatarImg { get; set; }
        public bool? isVegan { get; set; }
        public string? bio { get; set; }
        public List<string> categories { get; set; }
        public int hungryHeads { get; set; }
        public UserUpdateRequest()
        {
            Console.WriteLine("request created");
            fullName = string.Empty;
            googleLink = string.Empty;
            facebookLink = string.Empty;
            categories = new List<string>();
            avatarUrl = string.Empty;
            isVegan = false;
            bio = string.Empty;
            hungryHeads = 0;
        }
    }
}
