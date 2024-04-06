using CookingRecipeApi.Models;

namespace CookingRecipeApi.Repositories.Interfaces
{
    public interface INotificationBatchRepository
    {
        // read
        public Task<IEnumerable<Notification?>> GetNotifications(string userId, int clientPageOffSet);
        public Task<Notification> PushNotification(Notification notification, string userId);
        public Task UpdateNotification(Notification notification);
        public Task DeleteNotification(int notificationOffset);
    }
}
