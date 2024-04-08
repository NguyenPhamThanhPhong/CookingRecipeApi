using CookingRecipeApi.Configs;
using CookingRecipeApi.Hubs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Text.Json;

namespace CookingRecipeApi.Services.BusinessServices.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<NotificationBatch> _notificationBatchCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly INotificationBatchRepository _notificationBatchRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ProjectionDefinition<User,List<string>>? _projection = Builders<User>.Projection.Expression(x => x.followerIds);
        private readonly int _batchSize;
        public NotificationService(INotificationBatchRepository notificationBatchRepository,
            DatabaseConfigs databaseConfigs,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationBatchRepository = notificationBatchRepository;
            _userCollection = databaseConfigs.UserCollection;
            _notificationBatchCollection = databaseConfigs.NotificationBatchCollection;
            _hubContext = hubContext;
            _batchSize = databaseConfigs.NotificationBatchSize;
        }

        public Task<bool> DeleteNotification(int offSet, string userId)
        {
            int page = offSet / _batchSize;
            int item_offset = offSet % _batchSize;
            var filter = Builders<NotificationBatch>.Filter.Where(
                x => x.userId == userId 
                && x.page==page
                && x.notifications[item_offset]!=null );
            var update = Builders<NotificationBatch>.Update.Set(x => x.notifications[item_offset], null);
            return _notificationBatchCollection.UpdateOneAsync(filter, update)
                .ContinueWith(x => x.Result.ModifiedCount > 0);
        }

        public async Task<Tuple<string, List<string>>?> DetectUserIdtoNotification(string userId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.id, userId);
            var projection = Builders<User>.Projection.Expression(x => Tuple.Create(x.profileInfo.avatarUrl,x.followerIds));
            Tuple<string,List<string>>? followers = await _userCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            return followers;
        }

        public Task<IEnumerable<Notification?>> GetNotifications(string userId, int page)
        {
            return _notificationBatchRepository.GetNotifications(userId, page);
        }

        public  Task<bool> MarkRead(int offSet, string userId,bool isRead)
        {
            try
            {
                int page = offSet / _batchSize;
                var filter = Builders<NotificationBatch>.Filter.Where(
                    x => x.userId == userId
                    && x.page == page 
                    && x.notifications[offSet % _batchSize] != null);
                var update = Builders<NotificationBatch>.Update.Set(x => x.notifications[offSet % _batchSize].isRead, isRead);
                return _notificationBatchCollection.UpdateOneAsync(filter, update)
                    .ContinueWith(x=>x.Result.ModifiedCount>0);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(false);
            };
        }

        public async Task PushNotification(Notification notification,string userId)
        {
            try
            {
                var result = await _notificationBatchRepository.PushNotification(notification, userId);
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", result);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}
