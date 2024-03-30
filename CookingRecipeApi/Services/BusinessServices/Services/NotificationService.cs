using CookingRecipeApi.Configs;
using CookingRecipeApi.Hubs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<NotificationBatch> _notificationBatchCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly INotificationBatchRepository _notificationBatchRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationBatchRepository notificationBatchRepository,
            DatabaseConfigs databaseConfigs,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationBatchRepository = notificationBatchRepository;
            _userCollection = databaseConfigs.UserCollection;
            _notificationBatchCollection = databaseConfigs.NotificationBatchCollection;
            _hubContext = hubContext;
        }
        public async Task PushNotification(Notification notification,string userId)
        {
            try
            {
                var result = await _notificationBatchRepository.PushNotification(notification, userId);
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", result);
            }
            catch
            {

            }
        }


    }
}
