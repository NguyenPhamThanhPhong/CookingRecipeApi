﻿using CookingRecipeApi.Helper;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CookingRecipeApi.RequestsResponses.RecipeRequests
{
    public class RecipeUpdateRequest
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Required]
        // anotation validate string lenght 1-50
        [StringLength(50, MinimumLength = 1)]
        public string title { get; set; }
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string instruction { get; set; }
        public List<string>? keepUrls { get; set; }
        public List<IFormFile>? files { get; set; }
        [TimeSpanModelStateValidation]
        [SwaggerSchema(Format = "uint32", Description = "60")]
        public TimeSpan cookTime { get; set; }
        public Dictionary<string, string> ingredients { get; set; }
        public bool isPublished { get; set; }
        // must provide to do replace async
        public DateTime updatedTime { get; set; }

    }
}