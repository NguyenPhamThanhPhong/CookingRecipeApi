using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace CookingRecipeApi.Repositories.Repos
{
    public class NotificationBatchRepository : INotificationBatchRepository
    {
        private readonly IMongoCollection<NotificationBatch> _notificationBatchCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly int _notificationBatchSize;
        private readonly ClientConstants _clientConstants;
        public NotificationBatchRepository(DatabaseConfigs databaseConfigs, ClientConstants clientConstants)
        {
            _notificationBatchCollection = databaseConfigs.NotificationBatchCollection;
            _userCollection = databaseConfigs.UserCollection;
            _notificationBatchSize = databaseConfigs.NotificationBatchSize;
            _clientConstants = clientConstants;
        }

        public async Task DeleteNotification(int notificationOffset)
        {
            var page = notificationOffset / _notificationBatchSize;
            var offset = notificationOffset % _notificationBatchSize;
            var filter = Builders<NotificationBatch>.Filter.Eq(n => n.page, page);
            var update = Builders<NotificationBatch>.Update.Set(n => n.notifications[offset], null);
            await _notificationBatchCollection.UpdateOneAsync(filter, update);
        }


        public async Task<IEnumerable<Notification?>> GetNotifications(string userId, int page)
        {
            var filter = Builders<NotificationBatch>.Filter.Eq(n => n.userId, userId);
            var projection = Builders<NotificationBatch>.Projection.Expression(n => n.notifications);
            var sort = Builders<NotificationBatch>.Sort.Descending(n => n.page);
            // help me here copilot
            var manyNotifications = await _notificationBatchCollection
                .Find(filter).Project(projection).Sort(sort).Limit(2).Skip(page).ToListAsync();
            switch (manyNotifications?.Count)
            {
                case null:
                    return new List<Notification?>();
                case 0:
                    return new List<Notification?>();
                case 1:
                    return manyNotifications[0];
                default:
                    // if at least 2 pages
                    //check if first page has enough to return to client
                    // if return 2 pages
                    return (manyNotifications[0].Count < _clientConstants.NOTIFICATION_PAGE_MIN_SIZE) 
                        ? manyNotifications[0] : manyNotifications.SelectMany(n => n);
            }
        }

        public async Task<Notification> PushNotification(Notification notification, string userId)
        {
            Console.WriteLine("triggered");
            var filter = Builders<NotificationBatch>.Filter.And(
                Builders<NotificationBatch>.Filter.Eq(n => n.userId, userId));
            var sort = Builders<NotificationBatch>.Sort.Descending(n => n.page);

            UpdateResult updateResult;
            using(var cursor = await _notificationBatchCollection.FindAsync(filter, new FindOptions<NotificationBatch> { Sort = sort }))
            {
                var notificationBatch = await cursor.FirstOrDefaultAsync();
                if (notificationBatch == null)
                {
                    notification.offSet = 0;
                    notificationBatch = new NotificationBatch
                    {
                        userId = userId,
                        page = 0,
                        notifications = new List<Notification?> { notification }
                    };
                    await _notificationBatchCollection.InsertOneAsync(notificationBatch);
                    return notification;
                }
                if(notificationBatch.notifications.Count < _notificationBatchSize)
                {
                    notification.offSet = notificationBatch.notifications.Count;
                    var update = Builders<NotificationBatch>.Update.Push(n => n.notifications, notification);
                    updateResult = await _notificationBatchCollection.UpdateOneAsync(filter, update);
                    return notification;
                }
                else
                {
                    var lastPage = notificationBatch.page;

                    //đoạn này
                    var newPage = lastPage + 1;
                    // if the last page is full, then we need to create a new page
                    // example last page is 1, then new page is 2
                    //offset now is 20
                    notification.offSet = newPage * _notificationBatchSize;
                    var newNotificationBatch = new NotificationBatch
                    {
                        userId = userId,
                        page = newPage,
                        notifications = new List<Notification?> { notification }
                    };
                    await _notificationBatchCollection.InsertOneAsync(newNotificationBatch);
                    return notification;
                    //đoạn này


                    //var lastNotification = notificationBatch.notifications.Last();
                    //// why you check lastNotification is null?
                    //// if lastNotification is null, then the last page is not full
                    //// so we can add the notification to the last page
                    //if (lastNotification == null)
                    //{
                    //    notification.offSet = _notificationBatchSize - 1;
                    //    var update = Builders<NotificationBatch>.Update.Set(n => n.notifications[_notificationBatchSize - 1], notification);
                    //    updateResult = await _notificationBatchCollection.UpdateOneAsync(filter, update);
                    //    return notification;
                    //}
                    //else
                    //{
                        
                    //}   
                }
            }
        }

        public Task UpdateNotification(Notification notification)
        {
            var page = notification.offSet / _notificationBatchSize;
            var offset = notification.offSet % _notificationBatchSize;
            var filter = Builders<NotificationBatch>.Filter.Eq(n => n.page, page);
            var update = Builders<NotificationBatch>.Update.Set(n => n.notifications[offset], notification);
            return _notificationBatchCollection.UpdateOneAsync(filter, update);
        }
    }
}