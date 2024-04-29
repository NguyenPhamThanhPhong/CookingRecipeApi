using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.RequestsResponses.Requests.LoginRequests
{
#pragma warning disable CS8618
    public class RegisterWithEmailRequest : LoginRegisterRequestBase
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        public string? fullName { get; set; }
        public IFormFile? file { get; set; }
        public string? bio { get; set; }
    }
}
