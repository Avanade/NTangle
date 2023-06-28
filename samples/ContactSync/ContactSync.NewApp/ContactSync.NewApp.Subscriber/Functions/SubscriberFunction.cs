namespace ContactSync.NewApp.Subscriber.Functions;

public class SubscriberFunction
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber;

    public SubscriberFunction(ServiceBusOrchestratedSubscriber subscriber) => _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

    [Singleton(Mode = SingletonMode.Function)]
    [FunctionName("SubscriberFunction")]
    public Task Run([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] Az.ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
        => _subscriber.ReceiveAsync(message, messageActions, null, cancellationToken);
}