using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface INotificationBatchRepository
    {
        // read
        public Task<IEnumerable<Notification?>> GetNotifications(string userId, int clientPageOffSet);
        public Task<Notification> PushNotification(string userId, Notification notification);
        public Task UpdateNotification(string userId, Notification notification);
        public Task DeleteNotification(string userId, int notificationOffset);
    }
}
