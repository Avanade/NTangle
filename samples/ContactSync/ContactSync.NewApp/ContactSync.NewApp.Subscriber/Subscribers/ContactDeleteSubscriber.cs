namespace ContactSync.NewApp.Subscriber.Subscribers;

[EventSubscriber("old.contact", "deleted")]
public class ContactDeleteSubscriber : SubscriberBase<Contact>
{
    private readonly IDatabase _db;
    private static readonly Validator<Contact> _contactValidator = Validator.Create<Contact>().HasProperty(x => x.Id, p => p.Mandatory());

    public ContactDeleteSubscriber(IDatabase db)
    {
        _db = db.ThrowIfNull();
        ValueValidator = _contactValidator;
    }

    public override async Task<Result> ReceiveAsync(EventData<Contact> @event, EventSubscriberArgs args, CancellationToken cancellationToken)
        => (await _db.SqlStatement(Resource.GetStreamReader<ContactDeleteSubscriber>("ContactDelete.sql").ReadToEnd())
            .Param("ContactId", @event.Value.Id)
            .NonQueryWithResultAsync(cancellationToken)).AsResult();
}