/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace SqlServerDemo.Publisher.Data;

/// <summary>
/// Enables the Change Data Capture (CDC) <see cref="CustomerCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Customer]').
/// </summary>
public partial interface ICustomerOrchestrator : IEntityOrchestrator<CustomerCdc> { }

/// <summary>
/// Manages the Change Data Capture (CDC) <see cref="CustomerCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Customer]').
/// </summary>
public partial class CustomerOrchestrator : EntityOrchestrator<CustomerCdc, CustomerOrchestrator.CustomerCdcEnvelopeCollection, CustomerOrchestrator.CustomerCdcEnvelope, VersionTrackingMapper>, ICustomerOrchestrator
{
    private static readonly CustomerCdcMapper _customerCdcMapper = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerOrchestrator"/> class.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public CustomerOrchestrator(IDatabase db, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger<CustomerOrchestrator> logger) :
        base(db, "[NTangle].[spCustomerBatchExecute]", "[NTangle].[spCustomerBatchComplete]", eventPublisher, jsonSerializer, settings, logger) => CustomerOrchestratorCtor();

    partial void CustomerOrchestratorCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    protected override async Task<EntityOrchestratorResult<CustomerCdcEnvelopeCollection, CustomerCdcEnvelope>> GetBatchEntityDataAsync(CancellationToken cancellationToken = default)
    {
        var cColl = new CustomerCdcEnvelopeCollection();

        var result = await SelectQueryMultiSetAsync(MultiSetArgs.Create(
            // Root table: '[Legacy].[Cust]'
            new MultiSetCollArgs<CustomerCdcEnvelopeCollection, CustomerCdcEnvelope>(_customerCdcMapper, __result => cColl = __result, stopOnNull: true)), cancellationToken).ConfigureAwait(false);

        result.Result.AddRange(cColl);
        return result;
    }

    /// <inheritdoc/>
    protected override string EventSubject => "Legacy.Customer";

    /// <inheritdoc/>
    protected override EventSubjectFormat EventSubjectFormat => EventSubjectFormat.NameOnly;

    /// <inheritdoc/>
    protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

    /// <inheritdoc/>
    protected override string? EventType => "Legacy.Customer";

    /// <inheritdoc/>
    protected override Uri? EventSource => new("/database/cdc/legacy/cust", UriKind.Relative);

    /// <inheritdoc/>
    protected override EventSourceFormat EventSourceFormat { get; } = EventSourceFormat.NameAndTableKey;

    /// <inheritdoc/>
    protected override string[]? ExcludePropertiesFromETag => new string[] { "RowVersion" };

    /// <summary>
    /// Represents a <see cref="CustomerCdc"/> envelope to append the required (additional) database properties.
    /// </summary>
    public class CustomerCdcEnvelope : CustomerCdc, IEntityEnvelope
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public CdcOperationType DatabaseOperationType { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public byte[] DatabaseLsn { get; set; } = Array.Empty<byte>();

        /// <inheritdoc/>
        [JsonIgnore]
        public string? DatabaseTrackingHash { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public bool IsDatabasePhysicallyDeleted { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="CustomerCdcEnvelope"/> collection.
    /// </summary>
    public class CustomerCdcEnvelopeCollection : List<CustomerCdcEnvelope> { }

    /// <summary>
    /// Represents a <see cref="CustomerCdc"/> database mapper.
    /// </summary>
    public class CustomerCdcMapper : IDatabaseMapper<CustomerCdcEnvelope>
    {
        /// <inheritdoc/>
        public CustomerCdcEnvelope? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            Id = record.GetValue<int>("Id"),
            Name = record.GetValue<string?>("Name"),
            Email = record.GetValue<string?>("Email"),
            IsDeleted = record.GetValue<bool?>("IsDeleted"),
            RowVersion = record.GetValue<byte[]?>("RowVersion"),
            DatabaseOperationType = record.GetValue<CdcOperationType>(CdcOperationTypeColumnName),
            DatabaseLsn = record.GetValue<byte[]>(CdcLsnColumnName),
            DatabaseTrackingHash = record.GetValue<string?>(TrackingHashColumnName),
            IsDatabasePhysicallyDeleted = record.GetValue<bool>(IsPhysicallyDeletedColumnName)
        };

        /// <inheritdoc/>
        void IDatabaseMapper<CustomerCdcEnvelope>.MapToDb(CustomerCdcEnvelope? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}