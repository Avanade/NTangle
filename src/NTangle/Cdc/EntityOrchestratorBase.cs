// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Abstractions;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using NTangle.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides the base (internal) Change Data Capture (CDC) entity (aggregate root) orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TVersionTrackerMapper">The <see cref="VersionTracker"/> database mapper <see cref="Type"/>.</typeparam>
    public abstract class EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper> : IEntityOrchestrator
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TVersionTrackerMapper : IDatabaseMapper<VersionTracker>, new()
    {
        private const string MaxQuerySizeParamName = "MaxQuerySize";
        private const string ContinueWithDataLossParamName = "ContinueWithDataLoss";
        private const string BatchTrackingIdParamName = "BatchTrackingId";
        private const string VersionTrackingListParamName = "@VersionTrackingList";
        
        /// <summary>
        /// Gets the <see cref="IEntityEnvelope.DatabaseOperationType"/> column name.
        /// </summary>
        protected const string CdcOperationTypeColumnName = "_OperationType";

        /// <summary>
        /// Gets the <see cref="IEntityEnvelope.DatabaseLsn"/> column name.
        /// </summary>
        protected const string CdcLsnColumnName = "_Lsn";
        
        /// <summary>
        /// Gets the <see cref="IEntityEnvelope.DatabaseTrackingHash"/> column name.
        /// </summary>
        protected const string TrackingHashColumnName = "_TrackingHash";

        /// <summary>
        /// Gets the <see cref="IEntityEnvelope.IsDatabasePhysicallyDeleted"/> column name.
        /// </summary>
        protected const string IsPhysicallyDeletedColumnName = "_IsPhysicallyDeleted";

        private static readonly BatchTrackerMapper _batchTrackerMapper = new();
        private string? _name;
        private int _maxQuerySize = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> class with no identifier mapping support.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="executeStoredProcedureName">The name of the batch execute stored procedure.</param>
        /// <param name="completeStoredProcedureName">The name of the batch complete stored procedure.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        internal EntityOrchestratorBase(IDatabase db, string executeStoredProcedureName, string completeStoredProcedureName, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger)
        {
            Db = db.ThrowIfNull(nameof(db));
            ExecuteStoredProcedureName = executeStoredProcedureName.ThrowIfNull(nameof(executeStoredProcedureName));
            CompleteStoredProcedureName = completeStoredProcedureName.ThrowIfNull(nameof(completeStoredProcedureName));
            EventPublisher = eventPublisher.ThrowIfNull(nameof(eventPublisher));
            JsonSerializer = jsonSerializer.ThrowIfNull(nameof(jsonSerializer));
            Settings = settings.ThrowIfNull(nameof(settings));
            Logger = logger.ThrowIfNull(nameof(logger));

            MaxQuerySize = Settings.GetCdcValue<int?>(ServiceName, MaxQuerySizeParamName) ?? 100;
            ContinueWithDataLoss = Settings.GetCdcValue<bool?>(ServiceName, ContinueWithDataLossParamName) ?? false;
        }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Db { get; }

        /// <summary>
        /// Gets the name of the batch <b>execute</b> stored procedure.
        /// </summary>
        public string ExecuteStoredProcedureName { get; }

        /// <summary>
        /// Gets the name of the batch <b>complete</b> stored procedure.
        /// </summary>
        public string CompleteStoredProcedureName { get; }

        /// <summary>
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        public IEventPublisher EventPublisher { get; }

        /// <summary>
        /// Gets the <see cref="IJsonSerializer"/>.
        /// </summary>
        public IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="SettingsBase"/>.
        /// </summary>
        public SettingsBase Settings { get; }

        /// <summary>
        /// Gets the Execution Identifier for the current execution.
        /// </summary>
        /// <remarks>This value is managed/allocated internally.</remarks>
        protected Guid ExecutionId { get; private set; }

        /// <summary>
        /// Gets the service name (used for logging).
        /// </summary>
        protected virtual string ServiceName => _name ??= GetType().Name;

        /// <summary>
        /// Gets the <see cref="EventDataBase.Subject"/> (to be further formatted as per <see cref="EventSubjectFormat"/>).
        /// </summary>
        protected abstract string EventSubject { get; }

        /// <summary>
        /// Gets the <see cref="EventDataBase.Subject"/> <see cref="Events.EventSubjectFormat"/>.
        /// </summary>
        protected virtual EventSubjectFormat EventSubjectFormat { get; } = EventSubjectFormat.NameAndKey;

        /// <summary>
        /// Gets the <see cref="EventDataBase.Action"/> <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected virtual EventActionFormat EventActionFormat { get; } = EventActionFormat.None;

        /// <summary>
        /// Gets the <see cref="EventDataBase.Source"/>.
        /// </summary>
        protected virtual Uri? EventSource { get; }

        /// <summary>
        /// Gets the <see cref="EventDataBase.Source"/> <see cref="Events.EventSourceFormat"/>.
        /// </summary>
        protected virtual EventSourceFormat EventSourceFormat { get; } = EventSourceFormat.NameAndKey;

        /// <summary>
        /// Gets the <see cref="EventDataBase.Type"/>.
        /// </summary>
        protected virtual string? EventType { get; }

        /// <summary>
        /// Gets the list of property names that should be excluded from the serialized JSON <see cref="IETag"/> generation.
        /// </summary>
        protected virtual string[]? ExcludePropertiesFromETag { get; }

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="BatchTracker"/>.
        /// </summary>
        /// <remarks>Defaults to 100.</remarks>
        public int MaxQuerySize { get => _maxQuerySize; set => _maxQuerySize = value < 1 ? 100 : value; }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server </remarks>
        public bool ContinueWithDataLoss { get; set; }

        /// <summary>
        /// Gets or sets the delegate to perform additional processing on the consolidated set of entities.
        /// </summary>
        /// <remarks>This is invoked directly after consolidation, and prior to versioning and completion.</remarks>
        protected Func<TEntityEnvelopeColl, CancellationToken, Task>? AdditionalEnvelopeProcessingAsync { get; set; }

        /// <summary>
        /// Completes an existing batch updating the corresponding <paramref name="versionTracking"/> where appropriate.
        /// </summary>
        /// <param name="batchTrackerId">The <see cref="BatchTracker.Id"/>.</param>
        /// <param name="versionTracking">The <see cref="VersionTracker"/> list.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        Task<EntityOrchestratorResult> IEntityOrchestrator.CompleteAsync(long batchTrackerId, List<VersionTracker> versionTracking, CancellationToken cancellationToken)
            => CompleteAsync(batchTrackerId, versionTracking, cancellationToken);

        /// <summary>
        /// Completes an existing batch updating the corresponding <paramref name="versionTracking"/> where appropriate.
        /// </summary>
        /// <param name="batchTrackerId">The <see cref="BatchTracker.Id"/>.</param>
        /// <param name="versionTracking">The <see cref="VersionTracker"/> list.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        public async Task<EntityOrchestratorResult> CompleteAsync(long batchTrackerId, List<VersionTracker> versionTracking, CancellationToken cancellationToken = default)
        {
            if (ExecutionId == Guid.Empty)
                ExecutionId = Guid.NewGuid();

            var result = new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> { ExecutionId = ExecutionId };
            var msa = new MultiSetSingleArgs<BatchTracker>(_batchTrackerMapper, r => result.Batch = r, isMandatory: false, stopOnNull: true);

            try
            {
                await Db.StoredProcedure(CompleteStoredProcedureName).Params(p =>
                {
                    p.AddParameter(BatchTrackingIdParamName, batchTrackerId);
                    p.AddJsonParameter(VersionTrackingListParamName, versionTracking);
                }).SelectMultiSetAsync(MultiSetArgs.Create(msa), cancellationToken).ConfigureAwait(false);
            }
            catch (BusinessException bex) // The known (converted) SQL Exception.
            {
                result.Exception = bex;
                Logger.LogError("{Message}", result.Exception?.Message);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
                {
                    Logger.LogWarning("Task canceled.");
                    throw;
                }

                result.Exception = ex;
                Logger.LogCritical(ex, "Unexpected Exception encountered: {Message}", result.Exception.Message);
            }

            await Task.CompletedTask;
            return result;
        }

        /// <inheritdoc/>
        public async Task<EntityOrchestratorResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ExecutionId = Guid.NewGuid();
                return await ExecuteInternalAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ExecutionId = Guid.Empty;
            }
        }

        /// <summary>
        /// Performs the actual execution.
        /// </summary>
        private async Task<EntityOrchestratorResult> ExecuteInternalAsync(CancellationToken cancellationToken = default)
        { 
            // Get the requested batch data.
            EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result;
            Logger.LogDebug("Query for next (new) Change Data Capture batch. [MaxQuerySize={MaxQuerySize}, ContinueWithDataLoss={ContinueWithDataLoss}, ExecutionId={ExecutionId}]", MaxQuerySize, ContinueWithDataLoss, ExecutionId);

            var sw = Stopwatch.StartNew();

            try
            {
                result = await GetBatchEntityDataAsync(cancellationToken).ConfigureAwait(false);
                result.ExecutionId = ExecutionId;
                result.ExecuteStatus = new EntityOrchestratorExecuteStatus { InitialCount = result.Result.Count };
                sw.Stop();
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
                {
                    Logger.LogWarning("Task canceled. [ExecutionId={ExecutionId}]", ExecutionId);
                    throw;
                }

                result = new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> { Exception = ex, ExecutionId = ExecutionId };

                if (ex is BusinessException)
                    Logger.LogError("{DatabaseErrorMessage} [ExecutionId={ExecutionId}]", result.Exception.Message, ExecutionId);
                else
                    Logger.LogCritical(ex, "Unexpected Exception encountered: {ExceptionMessage} [ExecutionId={ExecutionId}]", result.Exception.Message, ExecutionId);

                return result;
            }

            if (result.Batch == null)
            {
                Logger.LogDebug("Batch 'none': No new Change Data Capture data was found. [ExecutionId={ExecutionId}]", ExecutionId);
                return result;
            }

            Logger.LogInformation("Batch '{BatchId}': {OperationsCount} entity operations(s) were found. [MaxQuerySize={MaxQuerySize}, ContinueWithDataLoss={ContinueWithDataLoss}, CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                result.Batch.Id, result.Result.Count, MaxQuerySize, ContinueWithDataLoss, result.Batch.CorrelationId, ExecutionId, sw.Elapsed.TotalMilliseconds);

            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogWarning("Batch '{BatchId}': Incomplete as a result of Cancellation. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}]", result.Batch.Id, result.Batch.CorrelationId, ExecutionId);
                return await Task.FromCanceled<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>>(cancellationToken).ConfigureAwait(false);
            }

            // Consolidate the results to the 'set' that is to be sent (i.e. ignore unneccessary).
            var coll = new TEntityEnvelopeColl();
            foreach (var grp in result.Result.GroupBy(x => new { x.EntityKey }))
            {
                // Find delete and use.
                var item = grp.Where(x => x.DatabaseOperationType == CdcOperationType.Delete).FirstOrDefault();

                // Where there is no delete operation then just use the first.
                if (item == null)
                {
                    item = grp.First();

                    // Confirm whether the record has been _subsequently_ deleted (physically), if so then skip for now as will be picked up later (this will maintain some semblance of the order of operations).
                    if (item.IsDatabasePhysicallyDeleted)
                        continue;
                }

                // Where supports logical delete and IsDeleted, then override DatabaseOperationType.
                if (item is ILogicallyDeletedExtended ld && ld.IsDeleted.HasValue && ld.IsDeleted.Value)
                {
                    item.DatabaseOperationType = CdcOperationType.Delete;
                    ld.ClearWhereDeleted();
                }

                coll.Add(item);
            }

            result.ExecuteStatus.ConsolidatedCount = coll.Count;

            // Check cancellation.
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogWarning("Batch '{BatchId}': Incomplete as a result of Cancellation. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}]", result.Batch.Id, result.Batch.CorrelationId, ExecutionId);
                return await Task.FromCanceled<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>>(cancellationToken).ConfigureAwait(false);
            }

            // Where additional envelope processing is required then execute.
            if (AdditionalEnvelopeProcessingAsync != null)
                await AdditionalEnvelopeProcessingAsync(coll, cancellationToken).ConfigureAwait(false);

            // Determine whether anything may have been sent before (version tracking) and exclude (i.e. do not send again).
            var coll2 = new TEntityEnvelopeColl();
            var tracking = new List<VersionTracker>();
            foreach (var item in coll)
            {
                var entity = item as TEntity;

                // Calculate the serialized hash for the ETag.
                if (ExcludePropertiesFromETag == null || ExcludePropertiesFromETag.Length == 0)
                    entity.ETag = ETagGenerator.Generate(JsonSerializer, entity);
                else
                {
                    JsonSerializer.TryApplyFilter(entity, ExcludePropertiesFromETag, out string json, JsonPropertyFilter.Exclude);
                    entity.ETag = ETagGenerator.GenerateHash(json);
                }

                // Where the ETag and TrackingHash match then skip (has already been published).
                if (item.DatabaseTrackingHash == null || item.DatabaseTrackingHash != entity.ETag)
                {
                    coll2.Add(item);
                    tracking.Add(new VersionTracker { Key = entity.EntityKey.ToString(), Hash = entity.ETag });
                }

                // Clear ETag where delete; i.e. non sensical.
                if (item.DatabaseOperationType == CdcOperationType.Delete && entity is IETag etag)
                    etag.ETag = null;
            }

            result.ExecuteStatus.PublishCount = coll2.Count;

            // Check cancellation.
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogWarning("Batch '{BatchId}': Incomplete as a result of Cancellation. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}]", result.Batch.Id, result.Batch.CorrelationId, ExecutionId);
                return await Task.FromCanceled<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>>(cancellationToken).ConfigureAwait(false);
            }

            // Publish & send the events.
            if (coll2.Count == 0)
                Logger.LogInformation("Batch '{BatchId}': No event(s) were published; non-unique version tracking hash and/or underlying data is physically deleted. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                    result.Batch.Id, result.Batch.CorrelationId, ExecutionId, sw.Elapsed.TotalMilliseconds);
            else
            {
                await CreateEventsAsync(EventPublisher, coll2, result.Batch.CorrelationId, cancellationToken).ConfigureAwait(false);
                sw = Stopwatch.StartNew();
                await EventPublisher.SendAsync(cancellationToken).ConfigureAwait(false);
                sw.Stop();

                Logger.LogInformation("Batch '{BatchId}': {EventCount} event(s) were published successfully. [Publisher={Publisher}, CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                    result.Batch.Id, result.ExecuteStatus.PublishCount, EventPublisher.GetType().Name, result.Batch.CorrelationId, ExecutionId, sw.Elapsed.TotalMilliseconds);
            }

            // Complete the batch (ignore any further 'cancel' as event(s) have been published and we *must* complete to minimise chance of sending more than once).
            sw = Stopwatch.StartNew();
            var cresult = await CompleteAsync(result.Batch.Id, tracking, cancellationToken).ConfigureAwait(false);
            cresult.ExecuteStatus = result.ExecuteStatus;
            sw.Stop();

            if (cresult.IsSuccessful)
                Logger.LogInformation("Batch '{BatchId}': Marked as Completed. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                    result.Batch.Id, result.Batch.CorrelationId, ExecutionId, sw.Elapsed.TotalMilliseconds);

            return cresult;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <paramref name="multiSetArgs"/>; whilst also outputing the resulting return value.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The resultant <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</returns>
        protected async Task<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>> SelectQueryMultiSetAsync(IEnumerable<IMultiSetArgs> multiSetArgs, CancellationToken cancellationToken = default)
        {
            var result = new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> { ExecutionId = ExecutionId };
            var msa = new List<IMultiSetArgs> { new MultiSetSingleArgs<BatchTracker>(_batchTrackerMapper, r => result.Batch = r, isMandatory: false, stopOnNull: true) };
            msa.AddRange(multiSetArgs);

            await Db.StoredProcedure(ExecuteStoredProcedureName).Params(p =>
            {
                p.AddParameter(MaxQuerySizeParamName, MaxQuerySize);
                p.AddParameter(ContinueWithDataLossParamName, ContinueWithDataLoss);
            }).SelectMultiSetAsync(msa, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets the batch entity data from the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <typeparamref name="TEntityEnvelopeColl"/>.</returns>
        protected abstract Task<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>> GetBatchEntityDataAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates and published none or more <see cref="EventData">events</see> from the entity <paramref name="coll">collection.</paramref>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="coll">The entity envelope collection.</param>
        /// <param name="correlationId">The correlarion identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks><para>This invokes the <see cref="CreateEventAsync{T}(IEventPublisher, T, CdcOperationType, string?, CancellationToken)"/> which does the <see cref="IEventPublisher.Publish(EventData[])"/> per item.</para>
        /// The <see cref="IEventPublisher.SendAsync(CancellationToken)"/> must <b>not</b> be performed as this will result in data publishing inconsistencies; this is managed internally by the <see cref="ExecuteAsync(CancellationToken)"/>.</remarks>
        protected virtual async Task CreateEventsAsync(IEventPublisher eventPublisher, TEntityEnvelopeColl coll, string? correlationId, CancellationToken cancellationToken = default)
        {
            foreach (var item in coll.ThrowIfNull(nameof(coll)))
            {
                await CreateEventAsync(eventPublisher, item, item.DatabaseOperationType, correlationId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates (and <see cref="IEventPublisher.Publish(EventData[])">publishes</see>) an <see cref="EventData"/> for the specified <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="value">The value.</param>
        /// <param name="operationType">The <see cref="CdcOperationType"/> to infer the <see cref="EventDataBase.Action"/>.</param>
        /// <param name="correlationId">The correlarion identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks>The <see cref="IEventPublisher.SendAsync(CancellationToken)"/> must <b>not</b> be performed as this will result in data publishing inconsistencies; this is managed internally by the <see cref="ExecuteAsync(CancellationToken)"/>.</remarks>
        protected virtual Task CreateEventAsync<T>(IEventPublisher eventPublisher, T value, CdcOperationType operationType, string? correlationId, CancellationToken cancellationToken = default) where T : IEntityKey
        {
            var subject = EventSubjectFormatter.Format(eventPublisher.EventDataFormatter, EventSubject, value, EventSubjectFormat);
            var action = EventActionFormatter.Format(operationType, EventActionFormat);
            var source = EventSourceFormatter.Format(eventPublisher.EventDataFormatter, EventSource, value, EventSourceFormat);

            var @event = source is null ? eventPublisher.CreateValueEvent(value, subject, action) : eventPublisher.CreateValueEvent(value, source, subject, action);
            @event.Type = EventType;
            @event.CorrelationId = correlationId;

            eventPublisher.Publish(@event);

            return Task.CompletedTask;
        }
    }
}