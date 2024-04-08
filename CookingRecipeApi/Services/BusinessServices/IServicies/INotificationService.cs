using CookingRecipeApi.Models;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface INotificationService
    {
        public Task PushNotification(Notification notification,string userId);
        public Task<Tuple<string,List<string>>?> DetectUserIdtoNotification(string userId);
        public Task<IEnumerable<Notification?>> GetNotifications(string userId, int page);
        public Task<bool> MarkRead(int offSet, string userId,bool isRead);
        public Task<bool> DeleteNotification(int offSet, string userId);
    }
}
