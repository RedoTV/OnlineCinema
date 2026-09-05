namespace OnlineCinema.Backend.Events;

/// <summary>
/// Публикует события в шину RabbitMQ.
/// В рантайме это fire-and-forget после коммита в БД (см. README —
/// честно признаю что outbox тут нет, для диплома ок).
/// </summary>
public interface IEventPublisher : IDisposable
{
    /// <summary>
    /// Публикует событие как JSON в exchange cinema.events по routingKey.
    /// Не выбрасывает исключений внутрь бизнес-логики — если шина лежит,
    /// событие молча теряется, а не валит запрос юзера.
    /// </summary>
    Task PublishAsync(string routingKey, object payload);
}
