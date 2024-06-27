using CookingRecipeApi.Helper;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.RequestsResponses.Requests.RecipeRequests
{
    public class RecipeUpdateRequest
    {
        [Required]
        public string id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Required]
        // anotation validate string lenght 1-50
        [StringLength(50, MinimumLength = 1)]
        public string title { get; set; }
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string instruction { get; set; }
        public string description { get; set; }
        public string categories { get; set; }
        public int serves { get; set; }
        [Required]
        public int representIndex { get; set; } = -1;
        public List<string>? keepUrls { get; set; }
        public List<IFormFile>? files { get; set; }
        [TimeSpanModelStateValidation]
        [SwaggerSchema(Format = "uint32", Description = "60")]
        public TimeSpan cookTime { get; set; } = TimeSpan.Zero;
        public List<string> ingredients { get; set; }
        public bool isPublished { get; set; } = false;
        // must provide to do replace async
        public bool isVegan { get; set; }

    }
}
