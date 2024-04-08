
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
        private readonly string _followerIdQueueName;
        private readonly string _notificationQueueName;
        private readonly SemaphoreSlim _notificationSemaphoreSlim;
        private readonly SemaphoreSlim _followerIdSemaphoreSlim;
        private readonly Func<Notification,string, Task> _sendNotification; // borrow this from service
        private readonly Func<string,Task<Tuple<string, List<string>>?>> _proccessFollowerId; // borrow this from service
        private readonly NotificationTaskProducer _notificationProducer;
        public NotificationTaskConsumer(MessageQueueConfigs messageQueueConfigs,
            NotificationTaskProducer notificationProducer,INotificationService notificationService)
        {
            _connection = messageQueueConfigs.Connection;
            _channel = _connection.CreateModel();
            _notificationQueueName = messageQueueConfigs.notificationQueueName;
            _followerIdQueueName = messageQueueConfigs.followerIdQueueName;
            _notificationSemaphoreSlim = new SemaphoreSlim(messageQueueConfigs.taskLimit);
            _followerIdSemaphoreSlim = new SemaphoreSlim(messageQueueConfigs.taskLimit);
            _sendNotification = notificationService.PushNotification;
            _proccessFollowerId = notificationService.DetectUserIdtoNotification;
            _notificationProducer = notificationProducer;
        }
        public void SetupConsumer()
        {
            // set up for followerid queue
            var followerIdConsumer = new EventingBasicConsumer(_channel);
            followerIdConsumer.Received += async (model, ea) =>
            {
                await _followerIdSemaphoreSlim.WaitAsync();
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var notificationMessage = System.Text.Json.JsonSerializer.Deserialize<NotificationMessage>(message);

                    if (notificationMessage == null)
                        return;

                    Tuple<string,List<string>>? metaData = await _proccessFollowerId(notificationMessage.UserId);
                    if (metaData == null)
                        return;
                    var followerIds = metaData.Item2;
                    var avatarUrl = metaData.Item1;
                    Task[] tasks = new Task[followerIds.Count()];
                    var generalNotification = new Notification
                    {
                        isRead = false,
                        imageUrl = avatarUrl,
                        redirectPath = notificationMessage.Path,
                        title = notificationMessage.Title,
                        content = notificationMessage.Message,
                        createdAt = DateTime.Now
                    };
                    for (int i = 0; i < followerIds.Count(); i++)
                    {
                        Console.WriteLine($"pushing ${followerIds.ElementAt(i)} with i=${i} ");
                        string tempid = followerIds.ElementAt(i);
                        tasks[i] = Task.Run(() => { _notificationProducer.EnqueueNotification(generalNotification, tempid); });
                    }
                    await Task.WhenAll(tasks);
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
            _channel.BasicConsume(queue: _followerIdQueueName, autoAck: true, consumer: followerIdConsumer);


            var notificationConsumer = new EventingBasicConsumer(_channel);
            notificationConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await _notificationSemaphoreSlim.WaitAsync();
                try
                {
                    var notificationTask = System.Text.Json.JsonSerializer.Deserialize<NotificationTask>(message);
                    Console.WriteLine($" [x] Received {0}", notificationTask?.UserId??"");
                    if (notificationTask != null)
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

        private void FollowerIdConsumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
    internal class NotificationTask
    {
        public NotificationTask(Notification notification, string userId)
        {
            Notification = notification;
            UserId = userId;
        }
        public Notification Notification { get; set; }
        public string UserId { get; set; }

    }

    public class NotificationMessage
    {
        public NotificationMessage(string message,string title, string userId,string path)
        {
            Message = message;
            UserId = userId;
            Title = title;
            Path = path;
        }
        public string Title { get; set; }
        public string Path { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}