using AutoMapper;
using CookingRecipeApi.Models;
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
        private readonly NotificationTaskProducer _notificationTaskProducer;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService notificationService, 
            IMapper mapper, NotificationTaskProducer notificationTaskProducer)
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
        [HttpPut("mark-read/{offSet}/{isRead}")]
        [Authorize]
        public async Task<IActionResult> MarkRead(int offSet, bool isRead)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            var result = await _notificationService.MarkRead(offSet, userId,isRead);
            return Ok(result);
        }
        [HttpPost("make-doom-notification-test/{message}")]
        public IActionResult DoomNotification([FromBody] List<string> userIds, string message)
        {
            var notification = new Notification()
            {
                content = message,
                createdAt = DateTime.UtcNow,
                isRead = false,
                offSet = 0,
                redirectPath = "/hello",
                title = "Doom Notification"
            };
            foreach (var userId in userIds)
            {
                _notificationTaskProducer.EnqueueNotification(notification,userId);
            }
            return Ok("NotificationController");
        }
    }
}
