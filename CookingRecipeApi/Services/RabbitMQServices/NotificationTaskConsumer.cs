
using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using CookingRecipeApi.Services.BusinessServices.IServicies;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CookingRecipeApi.Services.RabbitMQServices
{
    public class NotificationTaskConsumer 
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _notificationQueueName;
        private readonly SemaphoreSlim _notificationSemaphoreSlim;
        private readonly Func<Notification,string, Task> _sendNotification; // borrow this from service
        public NotificationTaskConsumer(MessageQueueConfigs messageQueueConfigs,
            NotificationTaskProducer notificationProducer,INotificationService notificationService)
        {
            _connection = messageQueueConfigs.Connection;
            _channel = _connection.CreateModel();
            _notificationQueueName = messageQueueConfigs.notificationQueueName;
            _notificationSemaphoreSlim = new SemaphoreSlim(messageQueueConfigs.taskLimit);
            _sendNotification = notificationService.PushNotification;
        }
        public void SetupConsumer()
        {
            Console.WriteLine("Setting up consumer");
            var notificationConsumer = new EventingBasicConsumer(_channel);
            notificationConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await _notificationSemaphoreSlim.WaitAsync();
                try
                {
                    Console.WriteLine("message");
                    var notificationTask = System.Text.Json.JsonSerializer.Deserialize<NotificationTask>(message);
                    if(notificationTask != null)
                    {
                        await _sendNotification(notificationTask.Notification, notificationTask.UserId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    _notificationSemaphoreSlim.Release();
                }
            };
            _channel.BasicConsume(queue: _notificationQueueName, autoAck: true, consumer: notificationConsumer);
        }

    }
}