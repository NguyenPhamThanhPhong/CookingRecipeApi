using AutoMapper;
using CookingRecipeApi.Models;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using CookingRecipeApi.Services.RabbitMQServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("${userId}/${page}")]
        public IActionResult Get(string userId,int page)
        {
            return Ok("NotificationController");
        }
        [HttpPost("Create")]
        public IActionResult Create([FromBody] string notification)
        {
            return Ok("NotificationController");
        }
        [HttpPost("doom-notification/${message}")]
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
