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

        public NotificationTaskProducer(MessageQueueConfigs messageQueueConfigs)
        {
            _connection = messageQueueConfigs.Connection;
            _channel = _connection.CreateModel();
            _notificationQueueName = messageQueueConfigs.notificationQueueName;
        }

        public void EnqueueNotification(Notification notification,string userId)
        {
            var message = JsonSerializer.Serialize(new NotificationTask(notification,userId));
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _notificationQueueName, basicProperties: null, body: body);
        }
    }
    //wrapper class
    public class NotificationTask
    {
        public NotificationTask(Notification notification, string userId)
        {
            Notification = notification;
            UserId = userId;
        }
        public Notification Notification { get; set; }
        public string UserId { get; set; }

    }
}