using Microsoft.AspNetCore.SignalR;

namespace CookingRecipeApi.Hubs
{
    public class NotificationHub : Hub
    {
        public Dictionary<string, List<string>> userConnections = new Dictionary<string, List<string>>();
    }
}
