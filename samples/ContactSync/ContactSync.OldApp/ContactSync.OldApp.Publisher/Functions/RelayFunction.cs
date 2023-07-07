namespace ContactSync.OldApp.Publisher.Functions;

public class RelayFunction
{
    private readonly EventOutboxService _eventOutboxService;

    public RelayFunction(EventOutboxService eventOutboxService) => _eventOutboxService = eventOutboxService.ThrowIfNull();

    [FunctionName(nameof(RelayFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _eventOutboxService.ExecuteAsync(cancellationToken);
}