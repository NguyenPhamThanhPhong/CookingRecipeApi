using CookingRecipeApi.Models;

namespace CookingRecipeApi.Services.BusinessServices.IServicies
{
    public interface INotificationService
    {
        public Task PushNotification(Notification notification,string userId);
    }
}
