namespace ContactSync.OldApp.Publisher.Functions
{
    public class PublishFunction
    {
        private readonly EventOutboxService _eventOutboxService;

        public PublishFunction(EventOutboxService eventOutboxService) => _eventOutboxService = eventOutboxService.ThrowIfNull();

        [FunctionName(nameof(PublishFunction))]
        public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _eventOutboxService.ExecuteAsync(cancellationToken);
    }
}
