using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CookingRecipeApi.Services.RabbitMQServices
{
    public class NotificationTaskProducer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _notificationQueueName;
        private readonly string _followerIdQueueName;

        public NotificationTaskProducer(MessageQueueConfigs messageQueueConfigs)
        {
            _connection = messageQueueConfigs.Connection;
            _channel = _connection.CreateModel();
            _notificationQueueName = messageQueueConfigs.notificationQueueName;
            _followerIdQueueName = messageQueueConfigs.followerIdQueueName;
        }
        //cái này sẽ đc trigger bởi consumer Phase 1
        public void EnqueueNotification(Notification notification,string userId)
        {
            var message = JsonSerializer.Serialize(new NotificationTask(notification,userId));
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _notificationQueueName, basicProperties: null, body: body);
        }
        //enqueue user publisher id: 
        //tìm tới DS các follower của user và enqueue vào queue thông báo (notification queue)
        //cái này sẽ đc trigger đầu tiên
        public void EnqueueUserPublisherId(string userId,
            string message = "",string title = "new recipe",
            string path="/")
        {
            var itemMessage = JsonSerializer.Serialize(
                new NotificationMessage(message:message,title:title,path:path,userId:userId));
            var body = Encoding.UTF8.GetBytes(itemMessage);
            _channel.BasicPublish(exchange: "", routingKey: _followerIdQueueName, basicProperties: null, body: body);
        }
    }
}