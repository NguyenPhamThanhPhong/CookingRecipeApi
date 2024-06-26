﻿using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Repositories.Interfaces;
using MongoDB.Driver;
using System.Text.Json;

namespace CookingRecipeApi.Repositories.Repos
{
    public class NotificationBatchRepository : INotificationBatchRepository
    {
        private readonly IMongoCollection<NotificationBatch> _notificationBatchCollection;
        private readonly int _notificationBatchSize;
        private readonly ClientConstants _clientConstants;
        public NotificationBatchRepository(DatabaseConfigs databaseConfigs, 
            ClientConstants clientConstants)
        {
            _notificationBatchCollection = databaseConfigs.NotificationBatchCollection;
            _notificationBatchSize = databaseConfigs.NotificationBatchSize;
            _clientConstants = clientConstants;
        }

        public async Task DeleteNotification(string userId, int notificationOffset)
        {
            var page = notificationOffset / _notificationBatchSize;
            var offset = notificationOffset % _notificationBatchSize;
            var filter = Builders<NotificationBatch>.Filter
                .Where(s => s.userId == userId && s.page == page);
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
                .Find(filter).Project(projection).Sort(sort).Limit(1).Skip(page).FirstOrDefaultAsync();
            if(manyNotifications == null)
                return new List<Notification?>();
            manyNotifications.Reverse();
            return manyNotifications;
            //switch (manyNotifications?.Count)
            //{
            //    case null:
            //        return new List<Notification?>();
            //    case 0:
            //        return new List<Notification?>();
            //    case 1:
            //        return manyNotifications[0];
            //    default: // list<list<notification>> 2 pages
            //        // if at least 2 pages
            //        //check if first page has enough to return to client
            //        // if so return 2 pages
            //        if (manyNotifications[0].Count < _clientConstants.NOTIFICATION_PAGE_MIN_SIZE)
            //        {
            //            var item = manyNotifications.SelectMany(n => n);
            //            return item;
            //        }
            //        else
            //        {
            //            return manyNotifications[0];
            //        }
            //        //return (manyNotifications[0].Count < _clientConstants.NOTIFICATION_PAGE_MIN_SIZE) 
            //        //    ? manyNotifications[0] : manyNotifications.SelectMany(n => n);
            //}
        }

        public async Task<Notification> PushNotification(string userId, Notification notification)
        {
            var filter = Builders<NotificationBatch>.Filter.Eq(n => n.userId, userId);
            var sort = Builders<NotificationBatch>.Sort.Descending(n => n.page);

            UpdateResult updateResult;
            using(var cursor = await _notificationBatchCollection.FindAsync(filter, new FindOptions<NotificationBatch> { Sort = sort }))
            {
                var notificationBatch = await cursor.FirstOrDefaultAsync();
                //Console.WriteLine(JsonSerializer.Serialize(notificationBatch));
                if (notificationBatch == null)
                {
                    notification.offSet = 0;
                    notificationBatch = new NotificationBatch
                    {
                        userId = userId,
                        count = 1,
                        page = 0,
                        notifications = new List<Notification?> { notification }
                    };
                    await _notificationBatchCollection.InsertOneAsync(notificationBatch);
                    return notification;
                }
                //Console.WriteLine($"batchsize is : {_notificationBatchSize}");
                if(notificationBatch.count < _notificationBatchSize)
                {
                    notification.offSet = notificationBatch.notifications.Count;
                    var update = Builders<NotificationBatch>.Update.Combine(
                        Builders<NotificationBatch>.Update.Push(n => n.notifications, notification),
                        Builders<NotificationBatch>.Update.Inc(n => n.count, 1));
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
                        count = 1,
                        notifications = new List<Notification?> { notification }
                    };
                    await _notificationBatchCollection.InsertOneAsync(newNotificationBatch);
                    return notification;
                }
            }
        }

        public Task UpdateNotification(string userId, Notification notification)
        {
            var page = notification.offSet / _notificationBatchSize;
            var offset = notification.offSet % _notificationBatchSize;
            var filter = Builders<NotificationBatch>.Filter
                .Where(s=>s.userId==userId && s.page==page);
            var update = Builders<NotificationBatch>.Update.Set(n => n.notifications[offset], notification);
            return _notificationBatchCollection.UpdateOneAsync(filter, update);
        }
    }
}