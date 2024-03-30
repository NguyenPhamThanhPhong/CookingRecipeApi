using RabbitMQ.Client;

namespace CookingRecipeApi.Configs
{
    public class MessageQueueConfigs : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IConnection Connection { get; set; }
        public IModel Channel { get; set; }
        // will be binded to appsettings.json
        public string hostName { get; set; }
        public string virtualHost { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int port { get; set; }
        public int taskLimit { get; set; }
        public string userIdQueueName { get; set; }
        public string notificationQueueName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public void Initialize()
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = port,
                VirtualHost = virtualHost,
                UserName = username,
                Password = password
            };
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            Channel.QueueDeclare(queue: userIdQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            Channel.QueueDeclare(queue: notificationQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
        public void Dispose()
        {
            Channel?.Dispose();
            Connection?.Dispose();
        }
    }
}
