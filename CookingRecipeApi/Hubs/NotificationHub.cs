using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CookingRecipeApi.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string,HashSet<string>> _userConnections 
            = new ConcurrentDictionary<string, HashSet<string>>();
        [Authorize]
        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            string connectionId = Context.ConnectionId;
            if(userId == null)
            {
                //refuse connection
                return Task.CompletedTask;
            }
            _userConnections.AddOrUpdate(userId, new HashSet<string> { connectionId }, (key, value) =>
            {
                value.Add(connectionId);
                return value;
            });
            return base.OnConnectedAsync();
        }
        [Authorize]
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            string connectionId = Context.ConnectionId;
            if (userId == null)
            {
                //refuse connection
                return Task.CompletedTask;
            }
            if(userId==null)
                return Task.CompletedTask;
            _userConnections.TryGetValue(userId, out HashSet<string>? connections);
            connections?.Remove(connectionId);
            if (connections != null && connections.Count == 0)
            {
                _userConnections.TryRemove(userId, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }
        public static HashSet<string>? GetConnectionsFromUserId(string userId)
        {
            _userConnections.TryGetValue(userId, out HashSet<string>? connections);
            return connections;
        }
    }
}
