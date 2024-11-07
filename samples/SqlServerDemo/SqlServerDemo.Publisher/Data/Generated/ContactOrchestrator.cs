/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace SqlServerDemo.Publisher.Data;

/// <summary>
/// Enables the Change Data Capture (CDC) <see cref="ContactCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Contact]').
/// </summary>
public partial interface IContactOrchestrator : IEntityOrchestrator<ContactCdc> { }

/// <summary>
/// Manages the Change Data Capture (CDC) <see cref="ContactCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Contact]').
/// </summary>
public partial class ContactOrchestrator : EntityOrchestrator<ContactCdc, ContactOrchestrator.ContactCdcEnvelopeCollection, ContactOrchestrator.ContactCdcEnvelope, string>, IContactOrchestrator
{
    private static readonly ContactCdcMapper _contactCdcMapper = new();
    private static readonly AddressCdcMapper _addressCdcMapper = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactOrchestrator"/> class.
    /// </summary>
    /// <param name="database">The <see cref="IDatabase"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="idGen">The <see cref="IIdentifierGenerator{T}"/>.</param>
    public ContactOrchestrator(IDatabase database, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger<ContactOrchestrator> logger, IIdentifierGenerator<string> idGen)
        : base(database, eventPublisher, jsonSerializer, settings, logger, idGen) => ContactOrchestratorCtor();

    partial void ContactOrchestratorCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    protected override string ExecuteStoredProcedureName => "[NTangle].[spContactBatchExecute]";

    /// <inheritdoc/>
    protected override string CompleteStoredProcedureName => "[NTangle].[spContactBatchComplete]";

    /// <inheritdoc/>
    protected override string IdentifierMappingStoredProcedureName => "[NTangle].[spIdentifierMappingCreate]";

    /// <inheritdoc/>
    protected override async Task GetBatchEntityDataAsync(EntityOrchestratorResult<ContactCdcEnvelopeCollection, ContactCdcEnvelope> result, CancellationToken cancellationToken = default)
    {
        var cColl = new ContactCdcEnvelopeCollection();

        await SelectQueryMultiSetAsync(result, MultiSetArgs.Create(
            // Root table: '[Legacy].[Contact]'
            new MultiSetCollArgs<ContactCdcEnvelopeCollection, ContactCdcEnvelope>(_contactCdcMapper, __result => cColl = __result, stopOnNull: true),

            // Join table: '[Legacy].[Address]' (unique name 'Address')
            new MultiSetCollArgs<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>(_addressCdcMapper, __result =>
            {
                foreach (var a in __result.GroupBy(x => new { x.AID }).Select(g => new { g.Key.AID, Coll = g.ToCollection<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>() }))
                {
                    cColl.Where(x => x.AddressId == a.AID).ForEach(x => x.Address = a.Coll.FirstOrDefault());
                }
            })), cancellationToken).ConfigureAwait(false);

        result.Result.AddRange(cColl);
    }

    /// <inheritdoc/>
    protected override string Schema => "Legacy";

    /// <inheritdoc/>
    protected override string Table => "Contact";

    /// <inheritdoc/>
    protected override string EventSubject => "Legacy.Contact";

    /// <inheritdoc/>
    protected override EventSubjectFormat EventSubjectFormat => EventSubjectFormat.NameOnly;

    /// <inheritdoc/>
    protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

    /// <inheritdoc/>
    protected override string? EventType => "Legacy.Contact";

    /// <inheritdoc/>
    protected override Uri? EventSource => new("/database/cdc/legacy/contact", UriKind.Relative);

    /// <inheritdoc/>
    protected override EventSourceFormat EventSourceFormat { get; } = EventSourceFormat.NameAndTableKey;

    /// <summary>
    /// Represents a <see cref="ContactCdc"/> envelope to append the required (additional) database properties.
    /// </summary>
    public class ContactCdcEnvelope : ContactCdc, IEntityEnvelope
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public CdcOperationType DatabaseOperationType { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public byte[] DatabaseLsn { get; set; } = [];

        /// <inheritdoc/>
        [JsonIgnore]
        public string? DatabaseTrackingHash { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public bool IsDatabasePhysicallyDeleted { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="ContactCdcEnvelope"/> collection.
    /// </summary>
    public class ContactCdcEnvelopeCollection : List<ContactCdcEnvelope> { }

    /// <summary>
    /// Represents a <see cref="ContactCdc"/> database mapper.
    /// </summary>
    public class ContactCdcMapper : IDatabaseMapper<ContactCdcEnvelope>
    {
        /// <inheritdoc/>
        public ContactCdcEnvelope? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            GlobalId = record.GetValue<string?>("GlobalId"),
            CID = record.GetValue<int>("CID"),
            Name = record.GetValue<string?>("Name"),
            Phone = record.GetValue<string?>("Phone"),
            Email = record.GetValue<string?>("Email"),
            Active = record.GetValue<bool?>("Active"),
            DontCallList = record.GetValue<bool?>("DontCallList"),
            AddressId = record.GetValue<int?>("AddressId"),
            AlternateContactId = record.GetValue<int?>("AlternateContactId"),
            GlobalAlternateContactId = record.GetValue<string?>("GlobalAlternateContactId"),
            UniqueId = record.GetValue<Guid>("UniqueId"),
            DatabaseOperationType = record.GetValue<CdcOperationType>(CdcOperationTypeColumnName),
            DatabaseLsn = record.GetValue<byte[]>(CdcLsnColumnName),
            DatabaseTrackingHash = record.GetValue<string?>(TrackingHashColumnName),
            IsDatabasePhysicallyDeleted = record.GetValue<bool>(IsPhysicallyDeletedColumnName)
        };

        /// <inheritdoc/>
        void IDatabaseMapper<ContactCdcEnvelope>.MapToDb(ContactCdcEnvelope? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a <see cref="AddressCdc"/> database mapper.
    /// </summary>
    public class AddressCdcMapper : IDatabaseMapper<ContactCdc.AddressCdc>
    {
        /// <inheritdoc/>
        public ContactCdc.AddressCdc? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            AID = record.GetValue<int>("AID"),
            Street1 = record.GetValue<string?>("Street1"),
            Street2 = record.GetValue<string?>("Street2"),
            AlternateAddressId = record.GetValue<int?>("AlternateAddressId"),
            GlobalAlternateAddressId = record.GetValue<string?>("GlobalAlternateAddressId")
        };

        /// <inheritdoc/>
        void IDatabaseMapper<ContactCdc.AddressCdc>.MapToDb(ContactCdc.AddressCdc? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}