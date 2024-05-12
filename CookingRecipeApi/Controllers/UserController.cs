using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.UserRequests;
using CookingRecipeApi.RequestsResponses.Responses;
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
        [HttpGet()]
        public async Task<IActionResult> GetSelfInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("NOT AUTHENTICATED");
            var user = await _userService.getSelf(userId);
            return Ok(user);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfilebyId(string id)
        {
            UserProfileResponse profile = await _userService.getProfilebyId(id);
            return Ok(profile);
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] int page)
        {
            IEnumerable<UserProfileResponse> profiles = await _userService.getProfileSearch(searchTerm??"", page);
            return Ok(profiles);
        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm]UserUpdateRequest request)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null)
                return Unauthorized("");
            ProfileInformation result = await _userService.UpdateProfileBasicbyId(request, userID);
            return Ok(result);
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
//[Authorize]
//[HttpPut("update-profile-optional")]
//public async Task<IActionResult> UpdateProfileOptional([FromForm] UserUpdateProfileOptional request)
//{
//    var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//    if (userID == null)
//        return Unauthorized("");
//    var result = await _userService.UpdateProfileOptionalbyId(request, userID);
//    return Ok(result);
//}