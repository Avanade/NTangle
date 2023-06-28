namespace ContactSync.NewApp.Subscriber.Subscribers;

[EventSubscriber("old.contact", "created", "updated")]
public class ContactUpsertSubscriber : SubscriberBase<Contact>
{
    private readonly IDatabase _db;

    private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
        .HasProperty(x => x.Street1, p => p.Mandatory().String(200))
        .HasProperty(x => x.Street2, p => p.String(200));

    private static readonly Validator<Contact> _contactValidator = Validator.Create<Contact>()
        .HasProperty(x => x.Id, p => p.Mandatory())
        .HasProperty(x => x.Name, p => p.Mandatory().String(200))
        .HasProperty(x => x.Phone, p => p.String(50))
        .HasProperty(x => x.Email, p => p.String(200).Email())
        .HasProperty(x => x.Address, p => p.Entity(_addressValidator));

    public ContactUpsertSubscriber(IDatabase db)
    {
        _db = db.ThrowIfNull();
        ValueValidator = _contactValidator;
    }

    public override async Task<Result> ReceiveAsync(EventData<Contact> @event, EventSubscriberArgs args, CancellationToken cancellationToken)
        => (await _db.SqlStatement(Resource.GetStreamReader<ContactUpsertSubscriber>("ContactUpsert.sql").ReadToEnd())
            .Param("ContactId", @event.Value.Id)
            .Param("Name", @event.Value.Name)
            .Param("Phone", @event.Value.Phone)
            .Param("Email", @event.Value.Email)
            .Param("IsActive", @event.Value.IsActive)
            .Param("NoCallList", @event.Value.NoCallList)
            .Param("AddressStreet1", @event.Value.Address?.Street1)
            .Param("AddressStreet2", @event.Value.Address?.Street2)
            .NonQueryWithResultAsync(cancellationToken)).AsResult();
}