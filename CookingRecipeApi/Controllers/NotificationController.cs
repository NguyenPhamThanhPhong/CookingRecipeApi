using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.RequestsResponses.Requests.NotificationRequestsDTO;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookingRecipeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly NotificationTaskProducer _notificationTaskProducer;

        public NotificationController(INotificationService notificationService,
            IMapper mapper,
            NotificationTaskProducer notificationTaskProducer)
        {
            _notificationService = notificationService;
            _mapper = mapper;
            _notificationTaskProducer = notificationTaskProducer;
        }
        [HttpGet("{page}")]
        [Authorize]
        public async Task<IActionResult> Get(int page)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
            {
                return BadRequest("User not found");
            }
            var notifications = await _notificationService.GetNotifications(userId, page);
            return Ok(notifications);
        }
        [HttpPut("update-isread/{offSet}/{isRead}")]
        [Authorize]
        public async Task<IActionResult> MarkRead(int offSet, bool isRead)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            var result = await _notificationService.MarkRead(offSet, userId,isRead);
            Console.WriteLine(result);
            return result?Ok():BadRequest("not found user");
        }
        [HttpDelete("{offSet}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(int offSet)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            var result = await _notificationService.DeleteNotification(offSet, userId);
            return result ? Ok() : BadRequest("not found user");
        }
        [HttpPost("create-notifications")]
        public IActionResult DoomNotification([FromBody] NotificationCreateRequest request)
        {
            var notification = new Notification()
            {
                offSet = 0,
                title = request.title,
                content = request.content,
                createdAt = DateTime.UtcNow,
                isRead = false,
                redirectPath = request.redirectPath,
            };
            foreach (var userId in request.userIds)
            {
                _notificationTaskProducer.EnqueueNotification(notification, userId);
            }
            return Ok("NotificationController");
        }
    }
}
