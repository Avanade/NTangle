namespace AppName.Publisher.Functions;

public class ContactFunction(ContactService contactService)
{
    private readonly ContactService _contactService = contactService.ThrowIfNull();

    [Function(nameof(ContactFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _contactService.ExecuteAsync(cancellationToken);
}