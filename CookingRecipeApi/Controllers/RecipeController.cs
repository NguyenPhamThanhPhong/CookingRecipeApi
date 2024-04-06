﻿using AutoMapper;
using CookingRecipeApi.RequestsResponses.RecipeRequests;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using CookingRecipeApi.Services.BusinessServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

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
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized();

            var recipe = await _recipeService.GetRecipeById(id);

            if (recipe == null)
                return NotFound("recipe not found in database");
            return Ok(recipe);
        }
        [HttpPost("get-many")]
        public async Task<IActionResult> GetMany([FromQuery] IEnumerable<string> ids)
        {
            var recipes = await _recipeService.GetRecipes(ids);
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
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRecipe([FromForm] RecipeUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("request invalid");
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userID == null || name == null)
                return Unauthorized();
            var recipe = await _recipeService.UpdateRecipe(request, userID);
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
        [Authorize]
        [HttpGet("detect-userId/for-test-only")]
        [SwaggerOperation(Summary = "Detect user id to notification", 
            Description = "Detect user id to notification", OperationId = "DetectUserIdtoNotification")]
        public async Task<IActionResult> DetectUserIdtoNotification()
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized();
            var result = await _notificationService.DetectUserIdtoNotification(userID);
            return Ok(result);
        }

    }
}