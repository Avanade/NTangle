namespace AppName.Publisher.Functions;

public class ContactFunction
{
    private readonly ContactService _contactService;

    public ContactFunction(ContactService contactService) => _contactService = contactService.ThrowIfNull();

    [Function(nameof(ContactFunction))]
    public Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timer, CancellationToken cancellationToken) => _contactService.ExecuteAsync(cancellationToken);
}