// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Events;
using CoreEx.Json;
using CoreEx;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreEx.Entities;
using System.Threading;
using NTangle.Events;
using CoreEx.Database;
using System.Linq;
using CoreEx.Abstractions;
using CoreEx.Results;
using System.Diagnostics;

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides the core (internal) Change Data Capture (CDC) entity (aggregate root) orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    public abstract class EntityOrchestratorCore<TEntity, TEntityEnvelopeColl, TEntityEnvelope> : IEntityOrchestrator
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
    {
        /// <summary>
        /// Gets the <c>MaxQuerySize</c> parameter name.
        /// </summary>
        protected const string MaxQuerySizeParamName = "MaxQuerySize";

        /// <summary>
        /// Gets the <c>ContinueWithDataLoss</c> parameter name.
        /// </summary>
        protected const string ContinueWithDataLossParamName = "ContinueWithDataLoss";

        /// <summary>
        /// Gets the <c>BatchTrackingId</c> parameter name.
        /// </summary>
        protected const string BatchTrackingIdParamName = "BatchTrackingId";

        /// <summary>
        /// Gets the <c>VersionTrackingList</c> parameter name.
        /// </summary>
        protected const string VersionTrackingListParamName = "VersionTrackingList";

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
        /// Initializes a new instance of the <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope}"/> class with no identifier mapping support.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        internal EntityOrchestratorCore(IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger)
        {
            EventPublisher = eventPublisher.ThrowIfNull(nameof(eventPublisher));
            JsonSerializer = jsonSerializer.ThrowIfNull(nameof(jsonSerializer));
            Settings = settings.ThrowIfNull(nameof(settings));
            Logger = logger.ThrowIfNull(nameof(logger));

            MaxQuerySize = Settings.GetCdcValue<int?>(ServiceName, MaxQuerySizeParamName) ?? 100;
            ContinueWithDataLoss = Settings.GetCdcValue<bool?>(ServiceName, ContinueWithDataLossParamName) ?? false;
        }

        /// <summary>
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        protected IEventPublisher EventPublisher { get; }

        /// <summary>
        /// Gets the <see cref="IJsonSerializer"/>.
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="SettingsBase"/>.
        /// </summary>
        protected SettingsBase Settings { get; }

        /// <inheritdoc/>
        /// <remarks>This value is managed/allocated internally.</remarks>
        public Guid ExecutionId { get; internal protected set; }

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
        /// <remarks>For more information as to why data loss may occur see <see href="https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server"/>.</remarks>
        public bool ContinueWithDataLoss { get; set; }

        /// <summary>
        /// Gets the <see cref="BatchTrackerMapper"/>.
        /// </summary>
        protected BatchTrackerMapper BatchTrackerMapper => _batchTrackerMapper;

        /// <summary>
        /// Gets or sets the delegate to perform post processing on the consolidated set of entities.
        /// </summary>
        protected internal Func<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>, CancellationToken, Task>? PostConsolidationProcessingAsync { get; set; }

        /// <summary>
        /// Gets or sets the delegate to perform additional processing on the consolidated set of entities.
        /// </summary>
        /// <remarks>This is invoked directly after consolidation, and prior to versioning and completion.</remarks>
        protected Func<EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>, CancellationToken, Task>? AdditionalProcessingAsync { get; set; }

        /// <inheritdoc/>
        public abstract Task<EntityOrchestratorResult> ExecuteAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<EntityOrchestratorResult> CompleteAsync(long outboxId, List<VersionTracker> tracking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes an existing batch updating the corresponding <paramref name="versionTracking"/> where appropriate.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/>.</param>
        /// <param name="completeStoredProcedureName">The complete stored procedure name.</param>
        /// <param name="batchTrackerId">The <see cref="BatchTracker.Id"/>.</param>
        /// <param name="versionTracking">The <see cref="VersionTracker"/> list.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        protected async Task<EntityOrchestratorResult> CompleteAsync(IDatabase database, string completeStoredProcedureName, long? batchTrackerId, List<VersionTracker> versionTracking, CancellationToken cancellationToken = default)
        {
            var resetExecutionId = false;
            if (ExecutionId == Guid.Empty)
            {
                ExecutionId = Guid.NewGuid();
                resetExecutionId = true;
            }

            var eor = await EntityOrchestratorInvoker.Current.InvokeAsync(this, async (_, ct) =>
            { 
                var stopwatch = Stopwatch.StartNew();
                var result = new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> { ExecutionId = ExecutionId };

                try
                {
                    var cmd = database.ThrowIfNull(nameof(database)).StoredProcedure(completeStoredProcedureName.ThrowIfNullOrEmpty(nameof(completeStoredProcedureName)))
                        .Param(BatchTrackingIdParamName, batchTrackerId)
                        .JsonParam(VersionTrackingListParamName, versionTracking);

                    if (batchTrackerId is null)
                        await cmd.NonQueryAsync(ct).ConfigureAwait(false);
                    else
                        await cmd.SelectMultiSetAsync(MultiSetArgs.Create(new MultiSetSingleArgs<BatchTracker>(BatchTrackerMapper, r => result.BatchTracker = r, isMandatory: false, stopOnNull: true)), ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var eresult = ExceptionHandler(result, ex);
                    if (eresult is not null)
                        return eresult;

                    throw;
                }

                stopwatch.Stop();
                if (result.IsSuccessful)
                    Logger.LogInformation("{Service}: Batch '{BatchId}': Marked as Completed. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                        ServiceName, result.BatchTracker?.Id.ToString() ?? "n/a", result.BatchTracker?.CorrelationId ?? ExecutionId.ToString(), ExecutionId, stopwatch.Elapsed.TotalMilliseconds);

                return result;
            }, cancellationToken).ConfigureAwait(false);

            if (resetExecutionId)
                ExecutionId = Guid.Empty;

            return eor;
        }

        /// <summary>
        /// Execution invoker and exception handler.
        /// </summary>
        /// <returns>The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/> with the <see cref="Exception"/> recorded; otherwise, <c>null</c> being to bubble the originating exception.</returns>
        protected internal EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>? ExceptionHandler(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope>? result, Exception ex)
        {
            if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
            {
                Logger.LogWarning("{Service}: Task canceled. [ExecutionId={ExecutionId}]", ServiceName, ExecutionId);
                return null;
            }

            result ??= new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> {ExecutionId = ExecutionId };
            result.Exception = ex;

            if (ex is IExtendedException)
                Logger.LogError("{Service}: {ErrorMessage} [ExecutionId={ExecutionId}]", ServiceName, result.Exception.Message, ExecutionId);
            else
                Logger.LogCritical(ex, "Unexpected Exception encountered: {ExceptionMessage} [ExecutionId={ExecutionId}]", result.Exception.Message, ExecutionId);

            return result;
        }

        /// <summary>
        /// Logs the start of the query for the next (new) Change Data Capture batch.
        /// </summary>
        protected internal void LogQueryStart() 
            => Logger.LogDebug("{Service}: Query for next (new) Change Data Capture batch. [MaxQuerySize={MaxQuerySize}, ContinueWithDataLoss={ContinueWithDataLoss}, ExecutionId={ExecutionId}]",
                ServiceName, MaxQuerySize, ContinueWithDataLoss, ExecutionId);

        /// <summary>
        /// Logs the completion of the query for the next (new) Change Data Capture batch.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="stopwatch">The <see cref="Stopwatch"/>.</param>
        protected internal void LogQueryComplete(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, Stopwatch stopwatch)
        {
            if (result.BatchTracker == null)
                Logger.LogDebug("{Service}: Batch 'n/a': No new Change Data Capture data was found. [ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]", ServiceName, ExecutionId, stopwatch.Elapsed.TotalMilliseconds);
            else
                Logger.LogInformation("{Service}: Batch '{BatchId}': {OperationsCount} entity operations(s) were found. [MaxQuerySize={MaxQuerySize}, ContinueWithDataLoss={ContinueWithDataLoss}, CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                    ServiceName, result.BatchTracker.Id, result.Result.Count, MaxQuerySize, ContinueWithDataLoss, result.BatchTracker.CorrelationId, ExecutionId, stopwatch.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Consolidates the results by removing any unneccessary items.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected internal Task ConsolidateAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            EntityOrchestratorInvoker.Current.Invoke(this, _ =>
            {
                // Correct the explicit wonky operations not marked as deleted where actually physically deleted.
                if (result.IsExplicitExecution)
                    result.Result.Where(x => x.IsDatabasePhysicallyDeleted && x.DatabaseOperationType != CdcOperationType.Delete).ForEach(x => x.DatabaseOperationType = CdcOperationType.Delete);

                // Consolidate the results.
                var coll = new TEntityEnvelopeColl();
                foreach (var grp in result.Result.GroupBy(x => new { Key = x is IGlobalIdentifier gi ? gi.TableKey : x.EntityKey }))
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

                    // Where explicit and the item is being deleted check whether to assume delete where not found or ignore.
                    if (result.IsExplicitExecution && item.DatabaseOperationType == CdcOperationType.Delete && !result.ExplicitOptions!.AssumeDeleteWhereNotFound)
                        continue;

                    coll.Add(item);
                }

                result.SetResult(coll);
                result.ExecuteStatus!.ConsolidatedCount = result.Result.Count;
            });

            return Task.Delay(0, cancellationToken);
        }

        /// <summary>
        /// Post consolidation processing.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected internal async Task PostConsolidationAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            // Where internal post consolidation processing is required then execute.
            if (PostConsolidationProcessingAsync is not null)
            {
                await EntityOrchestratorInvoker.Current.InvokeAsync(this, async (_, ct) =>
                {
                    await PostConsolidationProcessingAsync(result, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Additional processing to further update the envelope collection.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected internal async Task AdditionalAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            // Where additional envelope processing is required then execute.
            if (AdditionalProcessingAsync is not null)
            {
                await EntityOrchestratorInvoker.Current.InvokeAsync(this, async (_, ct) =>
                {
                    await AdditionalProcessingAsync(result, ct).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Manages the versioning of the entities and returns the corresponding tracking information.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The corresponding <see cref="VersionTracker"/> list.</returns>
        protected internal async Task<List<VersionTracker>> VersionAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            var tracking = EntityOrchestratorInvoker.Current.Invoke(this, _ =>
            {
                var coll = new TEntityEnvelopeColl();
                var tracking = new List<VersionTracker>();

                foreach (var item in result.Result)
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
                    if ((result.IsExplicitExecution && result.ExplicitOptions!.AlwaysPublishEvents) || item.DatabaseTrackingHash == null || item.DatabaseTrackingHash != entity.ETag)
                    {
                        coll.Add(item);
                        tracking.Add(new VersionTracker { Key = entity is IGlobalIdentifier gi ? gi.TableKey.ToString() : entity.EntityKey.ToString(), Hash = entity.ETag });
                    }

                    // Clear ETag where delete; i.e. non sensical.
                    if (item.DatabaseOperationType == CdcOperationType.Delete && entity is IETag etag)
                        etag.ETag = null;
                }

                result.SetResult(coll);
                return tracking;
            });

            // Complete and exit.
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);
            return tracking;
        }

        /// <summary>
        /// Publishes and sends the events using the <see cref="EventPublisher"/>.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected internal Task PublishAndSendEventsAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            return EntityOrchestratorInvoker.Current.InvokeAsync(this, async (_, ct) =>
            {
                result.ExecuteStatus!.PublishCount = result.Result.Count;
                var correlationId = result.BatchTracker?.CorrelationId ?? result.ExecutionId.ToString();

                if (result.Result.Count == 0)
                    Logger.LogInformation("{Service}: Batch '{BatchId}': No event(s) were published; non-unique version tracking hash and/or underlying data is physically deleted. [CorrelationId={CorrelationId}, ExecutionId={ExecutionId}]",
                        ServiceName, result.BatchTracker?.Id.ToString() ?? "n/a", result.BatchTracker?.CorrelationId ?? ExecutionId.ToString(), ExecutionId);
                else
                {
                    await CreateEventsAsync(EventPublisher, result.Result, correlationId, cancellationToken).ConfigureAwait(false);

                    var sw = Stopwatch.StartNew();
                    await EventPublisher.SendAsync(cancellationToken).ConfigureAwait(false);
                    sw.Stop();

                    Logger.LogInformation("{Service}: Batch '{BatchId}': {EventCount} event(s) were published successfully. [Publisher={Publisher}, CorrelationId={CorrelationId}, ExecutionId={ExecutionId}, Elapsed={Elapsed}ms]",
                        ServiceName, result.BatchTracker?.Id.ToString() ?? "n/a", result.ExecuteStatus.PublishCount, EventPublisher.GetType().Name, correlationId, ExecutionId, sw.Elapsed.TotalMilliseconds);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Creates and published none or more <see cref="EventData">events</see> from the entity <paramref name="coll">collection.</paramref>.
        /// </summary>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="coll">The entity envelope collection.</param>
        /// <param name="correlationId">The correlarion identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <remarks><para>This invokes the <see cref="CreateEventAsync{T}(IEventPublisher, T, CdcOperationType, string?, CancellationToken)"/> which does the <see cref="IEventPublisher.Publish(EventData[])"/> per item.</para>
        /// <b>Note:</b> The <see cref="IEventPublisher.SendAsync(CancellationToken)"/> must <b>not</b> be performed as this will result in data publishing inconsistencies; this is managed internally.</remarks>
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
        /// <remarks><b>Note:</b> The <see cref="IEventPublisher.SendAsync(CancellationToken)"/> must <b>not</b> be performed as this will result in data publishing inconsistencies; this is managed internally.</remarks>
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