using AutoMapper;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using CookingRecipeApi.Services.BusinessServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;
using CookingRecipeApi.RequestsResponses.Requests.RecipeRequests;
using CookingRecipeApi.Models;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly NotificationTaskProducer _notificationTaskProducer;
        private readonly INotificationService _notificationService;

        public RecipeController(IRecipeService recipeService, NotificationTaskProducer notificationTaskProducer, INotificationService notificationService)
        {
            _recipeService = recipeService;
            _notificationTaskProducer = notificationTaskProducer;
            _notificationService = notificationService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(string id)
        {
            var recipe = await _recipeService.GetRecipeById(id);
            if (recipe == null)
                return NotFound("recipe not found in database");
            return Ok(recipe);
        }
        [Authorize]
        [HttpGet("get-from-likes")]
        public async Task<IActionResult> GetFeedRecipeFromLike()
        {
            bool isVegan = false;
            string? isVeganstr = User.FindFirst("isVegan")?.Value;
            if(isVeganstr!=null)
            {
                isVegan = bool.Parse(isVeganstr);
            }
            var recipes = await _recipeService.GetRecipesFromLikes(isVegan);
            return Ok(recipes);
        }
        [HttpGet("get-from-ids")]
        public async Task<IActionResult> GetMany([FromQuery] IEnumerable<string> recipeIds)
        {
            var recipes = await _recipeService
                .GetRecipesFromIds(recipeIds);
            return Ok(recipes);
        }
        [Authorize]
        [HttpGet("get-owned-recipes")]
        public async Task<IActionResult> GetOwned([FromQuery] List<string> categories, [FromQuery] string? searchTerm, [FromQuery] int page=0)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("userId not found in token");
            GetRecipeSearchRequest request = new GetRecipeSearchRequest() { categories = categories, searchTerm = searchTerm ?? "" };
            var recipes = await _recipeService.GetRecipesOwned(userID, request, page);
            return Ok(recipes);
        }
        [Authorize]
        [HttpGet("get-saved-recipes")]
        public async Task<IActionResult> GetSavedRecipes([FromQuery] List<string> categories, [FromQuery] string? searchTerm, [FromQuery] int page)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("userId not found in token");
            GetRecipeSearchRequest request = new GetRecipeSearchRequest() { categories = categories, searchTerm = searchTerm ?? "" };
            var recipes =  await _recipeService.GetRecipesSaved(userID, request,page);
            return Ok(recipes);
        }
        [Authorize]
        [HttpGet("get-liked-recipes")]
        public async Task<IActionResult> GetLikedRecipes([FromQuery] List<string> categories, [FromQuery] string? searchTerm, [FromQuery] int page)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("userId not found in token");
            GetRecipeSearchRequest request = new GetRecipeSearchRequest() { categories = categories, searchTerm = searchTerm ?? "" };
            var recipes = await _recipeService.GetRecipesLiked(userID, request, page);
            return Ok(recipes);
        }
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchbyCategory(
            [FromQuery] List<string> categories, [FromQuery] string? searchTerm, 
            [FromQuery] int page)
        {
            bool isVegan = false;
            string? isVeganstr = User.FindFirst("isVegan")?.Value;
            if (isVeganstr != null)
            {
                isVegan = bool.Parse(isVeganstr);
            }
            GetRecipeSearchRequest request = new GetRecipeSearchRequest() 
            { categories = categories, searchTerm = searchTerm ?? "", isVegan=isVegan};
            var recipes = await _recipeService.GetRecipesSearch(request, page);
            return Ok(recipes);
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeCreateRequest request)
        {
            if(!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userID == null|| name==null)
                return Unauthorized("shit");

            var recipe = await _recipeService.CreateRecipe(request, userID);
            if (recipe == null)
                return BadRequest("request invalid");

            //thêm publisherID vào RabbitMQ
            await _recipeService.NotifyRecipe(userID, name, recipe,RecipeNotificationType.Creation);
            return Ok(recipe);
        }
        [Authorize]
        [HttpPut("save-recipe/{recipeId}")]
        public async Task<IActionResult> SaveRecipe(string recipeId, [FromQuery] bool option)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized();

            var result = await _recipeService.SaveRecipe(userID, recipeId,option);
            return result ? Ok() : BadRequest();
        }
        [Authorize]
        [HttpPut("like-recipe/{recipeId}")]
        public async Task<IActionResult> LikeRecipe(string recipeId, [FromQuery] bool option)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized();

            var result = await _recipeService.LikeRecipe(userID, recipeId, option);
            return result ? Ok() : BadRequest();
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRecipe([FromForm] RecipeUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine($"userID={userID} name={name}");
            if (userID == null || name == null)
                return Unauthorized();
            Recipe? recipe = await _recipeService.UpdateRecipe(request, userID);
            if (recipe == null)
                return BadRequest("request invalid");
            await _recipeService.NotifyRecipe(userID, name, recipe, RecipeNotificationType.Update);
            return Ok(recipe);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized();
            await _recipeService.DeleteRecipe(id, userID);
            return Ok();
        }

    }
}
//[Authorize]
//[HttpGet("detect-userId/for-test-only")]
//[SwaggerOperation(Summary = "Detect user id to notification", 
//    Description = "Detect user id to notification", OperationId = "DetectUserIdtoNotification")]
//public async Task<IActionResult> DetectUserIdtoNotification()
//{
//    var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//    if (userID == null)
//        return Unauthorized();
//    var result = await _notificationService.DetectUserIdtoNotification(userID);
//    return Ok(result);
//}