using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentsMS.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace PaymentsMS.Infrastructure.EventBus
{
    public class EventBusProducer : BackgroundService, IEventBusProducer, IAsyncDisposable
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly RabbitMQSettings _rabbitmqSettings;

        public EventBusProducer(IOptions<RabbitMQSettings> option)
        {
            _rabbitmqSettings = option.Value;
            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            var basePath = AppContext.BaseDirectory;
            var pfxCertPath = Path.Combine(basePath, "Infrastructure", "Security", _rabbitmqSettings.CertFile);
            if (!File.Exists(pfxCertPath)) throw new FileNotFoundException("PFX certificate not found");

            var factory = new ConnectionFactory
            {
                HostName = _rabbitmqSettings.Host,
                UserName = _rabbitmqSettings.Username,
                Password = _rabbitmqSettings.Password,
                Port = _rabbitmqSettings.Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                ContinuationTimeout = TimeSpan.FromSeconds(5),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = _rabbitmqSettings.ServerName,
                    CertPath = pfxCertPath,
                    CertPassphrase = _rabbitmqSettings.CertPassphrase,
                    Version = System.Security.Authentication.SslProtocols.Tls12
                }
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }
        public async Task PublishEventAsync<TEvent>(TEvent eventMessage, string queueName)
        {
            if (_connection == null || !_connection.IsOpen || _channel.IsClosed)
            {
                await InitializeAsync();
            }

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMessage));
            BasicProperties props = new BasicProperties
            {
                Persistent = true,
            };

            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: props, body: messageBytes);
        }

        public async Task<TResponse> SendRequest<TResquest, TResponse>(TResquest resquest, string queueName)
        {
            if (_connection == null || !_connection.IsOpen || _channel.IsClosed)
            {
                await InitializeAsync();
            }
            await _channel.QueueDeclareAsync(
                queue: queueName, durable: true, 
                exclusive: false, autoDelete: false, 
                arguments: null
            );

            QueueDeclareOk replyQueue = await _channel.QueueDeclareAsync();
            string replyQueueName = replyQueue.QueueName;

            string correlationId = Guid.NewGuid().ToString();

            BasicProperties props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = replyQueueName
            };

            byte[] messageBytes =  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(props));
            await _channel.BasicPublishAsync(
                exchange: "", routingKey: queueName, 
                mandatory: false, basicProperties: props, 
                body: messageBytes
            );

            TaskCompletionSource<TResponse>? tcs = new TaskCompletionSource<TResponse>();

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var response = JsonConvert.DeserializeObject<TResponse>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                    tcs.SetResult(response);
                }
            };

            await _channel.BasicConsumeAsync(consumer: consumer, queue: replyQueueName, autoAck: false);

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            return await tcs.Task;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await InitializeAsync();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
                {
                    await InitializeAsync();
                }

                await Task.Delay(500, stoppingToken);
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
