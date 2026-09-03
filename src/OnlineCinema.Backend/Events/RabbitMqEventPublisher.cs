using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace OnlineCinema.Backend.Events;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchange = "cinema.events";
    private readonly string _uri;
    private bool _available;

    public RabbitMqEventPublisher(IConfiguration config, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        _uri = config["RabbitMq:Uri"] ?? "amqp://guest:guest@localhost:5672/";

        // брокера может не быть рядом (напр. обычный docker compose без rabbit) —
        // не падаем при старте, просто не публикуем. Прод требует reconnect, тут ок.
        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(_uri) };
            factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(5);

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: false, publisherConfirmationTrackingEnabled: false)).GetAwaiter().GetResult();

            _channel.ExchangeDeclareAsync(
                _exchange, type: "topic",
                durable: true, autoDelete: false,
                arguments: null,
                passive: false, noWait: false,
                CancellationToken.None).GetAwaiter().GetResult();

            _available = true;
            _logger.LogInformation("Event publisher на {Exchange} ({Uri})", _exchange, _uri);
        }
        catch (Exception ex)
        {
            _available = false;
            _logger.LogWarning(ex, "RabbitMQ недоступен ({Uri}), события не публикуются", _uri);
        }
    }

    public async Task PublishAsync(string routingKey, object payload)
    {
        if (!_available || _channel == null) return;

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        var props = new BasicProperties { ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };
        try
        {
            await _channel.BasicPublishAsync(_exchange, routingKey, mandatory: false, props, body, CancellationToken.None);
            _logger.LogDebug("Опубликовано {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            // не роняем запрос из-за шины
            _logger.LogError(ex, "Не удалось опубликовать {RoutingKey}", routingKey);
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
        }
        catch { /* игнор на закрытии */ }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
