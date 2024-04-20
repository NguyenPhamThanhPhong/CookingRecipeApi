using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;
using CookingRecipeApi.Services.AzureBlobServices;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AzureBlobHandler _azureBlobHandler;
        private readonly IUserService _userService;

        public UserController(AzureBlobHandler azureBlobHandler, IUserService userService)
        {
            _azureBlobHandler = azureBlobHandler;
            _userService = userService;
        }
        [HttpGet("get-from-rank")]
        public async Task<IActionResult> GetUsersfromRank()
        {
            var users = await _userService.GetUserFromFollowRank();
            return Ok(users);
        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm]UserUpdateRequest request)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("");
            var result = await _userService.UpdateProfilebyId(request, userID);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var profile = await _userService.getProfilebyId(id);
            return Ok(profile);
        }
        [Authorize]
        [HttpDelete("")]
        public async Task<IActionResult> DeleteUser()
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("");
            var result = await _userService.DeleteUser(userID);
            return Ok(result);
        }
        [Authorize]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] string password)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("");
            var result = await _userService.UpdatePassword(userID, password);
            return Ok(result);
        }
        [HttpGet("search/{search}/{skip}")]
        public async Task<IActionResult> Search(string search, int skip)
        {
            var profiles = await _userService.getProfileSearch(search,skip);
            return Ok(profiles);
        }
        [Authorize]
        [HttpPut("follow/{id}")]
        public async Task<IActionResult> Follow(string id,bool option)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("userID is {userID}");
            if (userID == null)
                return Unauthorized("");
            var result = await _userService.UpdateFollow(userID, id,option);
            return Ok(result);
        }

    }
}
