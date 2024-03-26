namespace AppName.Publisher.Functions;

public class PublishFunction(EventOutboxService eventOutboxService)
{
    private readonly EventOutboxService _eventOutboxService = eventOutboxService.ThrowIfNull();

    [Function(nameof(PublishFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _eventOutboxService.ExecuteAsync(cancellationToken);
}