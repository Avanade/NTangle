/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace SqlServerSidecarDemo.Publisher.Data;

/// <summary>
/// Enables the Change Data Capture (CDC) <see cref="CustomerCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Customer]').
/// </summary>
public partial interface ICustomerOrchestrator : IEntityOrchestrator<CustomerCdc> { }

/// <summary>
/// Manages the Change Data Capture (CDC) <see cref="CustomerCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Customer]').
/// </summary>
public partial class CustomerOrchestrator : EntitySidecarOrchestrator<CustomerCdc, CustomerOrchestrator.CustomerCdcEnvelopeCollection, CustomerOrchestrator.CustomerCdcEnvelope, CustomerOrchestrator.CustomerBatchTracker, CustomerOrchestrator.CustomerBatchTrackerMapper>, ICustomerOrchestrator
{
    private static readonly CustomerCdcMapper _customerCdcMapper = new();
    private static readonly CustomerBatchTrackerMapper _batchTrackerMapper = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerOrchestrator"/> class.
    /// </summary>
    /// <param name="database">The primary <see cref="IDatabase"/>.</param>
    /// <param name="sidecarDatabase">The sidecar <see cref="ISidecarDatabase"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public CustomerOrchestrator(IDatabase database, ISidecarDatabase sidecarDatabase, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger<CustomerOrchestrator> logger)
        : base(database, sidecarDatabase, eventPublisher, jsonSerializer, settings, logger) => CustomerOrchestratorCtor();

    partial void CustomerOrchestratorCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    protected override string ExecuteStoredProcedureName => "[NTangle].[spCustomerBatchExecute]";

    /// <inheritdoc/>
    protected override string CompleteStoredProcedureName => "[NTangle].[spCustomerBatchComplete]";

    /// <inheritdoc/>
    protected override string ExecuteCdcSqlResourceName => "Resources.Generated.CustomerExecuteBatch.sql";

    /// <inheritdoc/>
    protected override string TrackingStoredProcedureName => "[NTangle].[spCustomerBatchTracking]";

    /// <inheritdoc/>
    protected override async Task GetBatchEntityDataAsync(EntityOrchestratorResult<CustomerCdcEnvelopeCollection, CustomerCdcEnvelope> result, CancellationToken cancellationToken = default)
    {
        static void lsnSynchronizer(DatabaseRecord dr, CustomerBatchTracker bt)
        {
            bt.CustomerMinLsn = dr.GetValue<byte[]>("CustomerMinLsn");
            bt.CustomerMaxLsn = dr.GetValue<byte[]>("CustomerMaxLsn");
        }

        var cColl = new CustomerCdcEnvelopeCollection();

        await SelectQueryMultiSetAsync(result, MultiSetArgs.Create(
            // Root table: '[Legacy].[Cust]'
            new MultiSetCollArgs<CustomerCdcEnvelopeCollection, CustomerCdcEnvelope>(_customerCdcMapper, __result => cColl = __result, stopOnNull: true)), lsnSynchronizer, cancellationToken).ConfigureAwait(false);

        result.Result.AddRange(cColl);
    }

    /// <inheritdoc/>
    protected override string Schema => "Legacy";

    /// <inheritdoc/>
    protected override string Table => "Cust";

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
    protected override string[]? ExcludePropertiesFromETag => ["RowVersion"];

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
        public byte[] DatabaseLsn { get; set; } = [];

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
            IsDatabasePhysicallyDeleted = record.GetValue<bool>(IsPhysicallyDeletedColumnName)
        };

        /// <inheritdoc/>
        void IDatabaseMapper<CustomerCdcEnvelope>.MapToDb(CustomerCdcEnvelope? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents the 'Customer'-specific <see cref="BatchTracker"/>.
    /// </summary>
    public class CustomerBatchTracker : BatchTracker
    {
        /// <summary>
        /// Gets or sets the 'Customer' minimum LSN (Log Sequence Number).
        /// </summary>
        public byte[]? CustomerMinLsn { get; set; }

        /// <summary>
        /// Gets or sets the 'Customer' maximum LSN (Log Sequence Number).
        /// </summary>
        public byte[]? CustomerMaxLsn { get; set; }
    }

    /// <summary>
    /// Represents the <see cref="CustomerBatchTracker"/> database mapper.
    /// </summary>
    public class CustomerBatchTrackerMapper : DatabaseMapperEx<CustomerBatchTracker>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerBatchTrackerMapper"/> class.
        /// </summary>
        public CustomerBatchTrackerMapper() => InheritMapper(BatchTrackerMapper.Default);

        /// <inheritdoc/>
        protected override void OnMapFromDb(DatabaseRecord record, CustomerBatchTracker value, OperationTypes operationType)
        {
            value.CustomerMinLsn = record.GetValue<byte[]?>("CustomerMinLsn");
            value.CustomerMaxLsn = record.GetValue<byte[]?>("CustomerMaxLsn");
        }

        /// <inheritdoc/>
        protected override void OnMapToDb(CustomerBatchTracker value, DatabaseParameterCollection parameters, OperationTypes operationType)
        {
            parameters.AddParameter("CustomerMinLsn", value.CustomerMinLsn, dbType: System.Data.DbType.Binary);
            parameters.AddParameter("CustomerMaxLsn", value.CustomerMaxLsn, dbType: System.Data.DbType.Binary);
        }
    }
}