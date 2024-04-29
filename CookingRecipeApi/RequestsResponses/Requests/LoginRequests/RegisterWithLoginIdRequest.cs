using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.RequestsResponses.Requests.LoginRequests
{
#pragma warning disable CS8618
    public class RegisterWithLoginIdRequest : LoginRegisterRequestBase
    {
        [Required]
        public string loginId { get; set; }
        [Required]
        public string linkedAccountType { get; set; }
        public string? fullName { get; set; }
        public string? fileUrl { get; set; }
        public string? bio { get; set; }
    }
}
