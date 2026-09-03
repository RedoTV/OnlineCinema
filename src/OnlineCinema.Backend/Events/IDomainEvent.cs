namespace OnlineCinema.Backend.Events;

// Общий контракт событий для шины. Все события улетают в topic exchange
// "cinema.events" с routing key типа category.action (movie.rated и т.д.)
public interface IDomainEvent
{
    // человекочитаемый тип события, продублирует routing key
    string Type { get; }
    DateTime OccurredAt { get; }
}

// Обёртка, которую реально публикуем: содержательно routing key
// торчит отдельно, потому что RabbitMQ его в теле не хранит.
public record Envelope(string Type, string RoutingKey, DateTime OccurredAt, object Payload);
