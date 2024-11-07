// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides the base (internal) Change Data Capture (CDC) entity (aggregate root) sidecar-based orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TBatchTracker">The entity <see cref="BatchTracker"/>.</typeparam>
    /// <typeparam name="TBatchTrackerMapper">The entity <see cref="BatchTracker"/> mapper.</typeparam>
    public abstract class EntitySidecarOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TBatchTracker, TBatchTrackerMapper> : EntityOrchestratorCore<TEntity, TEntityEnvelopeColl, TEntityEnvelope>, IEntityOrchestrator
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TBatchTracker : BatchTracker, new()
        where TBatchTrackerMapper : class, IDatabaseMapperEx<TBatchTracker>, new()
    {
        private readonly TBatchTrackerMapper _batchTrackerMapper = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope}"/> class with no identifier mapping support.
        /// </summary>
        /// <param name="database">The primary <see cref="IDatabase"/>.</param>
        /// <param name="sidecarDatabase">The sidecar <see cref="ISidecarDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        internal EntitySidecarOrchestratorBase(IDatabase database, ISidecarDatabase sidecarDatabase, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger)
            : base(eventPublisher, jsonSerializer, settings, logger)
        {
            Database = database.ThrowIfNull(nameof(database));
            SidecarDatabase = sidecarDatabase.ThrowIfNull(nameof(sidecarDatabase));
        }

        /// <summary>
        /// Gets the primary <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; }

        /// <summary>
        /// Gets the sidecar <see cref="IDatabase"/>.
        /// </summary>
        public ISidecarDatabase SidecarDatabase { get; }

        /// <summary>
        /// Gets the name of the batch <b>execute</b> stored procedure.
        /// </summary>
        protected abstract string ExecuteStoredProcedureName { get; }

        /// <summary>
        /// Gets the name of the batch <b>execute CDC</b> SQL resource.
        /// </summary>
        protected abstract string ExecuteCdcSqlResourceName { get; }

        /// <summary>
        /// Gets the name of the batch <b>tracking</b> stored procedure.
        /// </summary>
        protected abstract string TrackingStoredProcedureName { get; }

        /// <summary>
        /// Gets the name of the batch <b>complete</b> stored procedure.
        /// </summary>
        protected abstract string CompleteStoredProcedureName { get; }

        /// <inheritdoc/>
        public override Task<EntityOrchestratorResult> CompleteAsync(long batchTrackerId, List<VersionTracker> versionTracking, CancellationToken cancellationToken = default)
            => CompleteAsync(SidecarDatabase, CompleteStoredProcedureName, batchTrackerId, versionTracking, cancellationToken);

        /// <inheritdoc/>
        public override async Task<EntityOrchestratorResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = new EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> { ExecutionId = ExecutionId = Guid.NewGuid() };
                return await EntityOrchestratorInvoker.Current.InvokeAsync(this, result, async (_, result, ct) => await ExecuteInternalAsync(result, ct).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ExecutionId = Guid.Empty;
            }
        }

        /// <summary>
        /// Performs the actual execution.
        /// </summary>
        private async Task<EntityOrchestratorResult> ExecuteInternalAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            try
            { 
                // Get the requested CDC batch data.
                LogQueryStart();
                var stopwatch = Stopwatch.StartNew();

                // Get the initial batch tracker information from the sidecar database to kick things off.
                await GetCurrentBatchTrackerAsync(result, cancellationToken).ConfigureAwait(false);

                // Perform the cdc-related querying within the primary database using the batch tracker information.
                await CdcBatchQueryAsync(result, cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                if (result.Batch == null)
                {
                    LogQueryComplete(result, stopwatch);
                    return result;
                }

                // Consolidate the results.
                await ConsolidateAsync(result, cancellationToken).ConfigureAwait(false);

                // Post-consolidation processing.
                await PostConsolidationAsync(result, cancellationToken).ConfigureAwait(false);

                // Can only log this now we know the batch id.
                LogQueryComplete(result, stopwatch);

                // Additional processing.
                await AdditionalAsync(result, cancellationToken).ConfigureAwait(false);

                // Determine whether anything may have been sent before (version tracking) and exclude (i.e. do not send again).
                var tracking = await VersionAsync(result, cancellationToken).ConfigureAwait(false);

                // Publish & send the events.
                await PublishAndSendEventsAsync(result, cancellationToken).ConfigureAwait(false);

                // Complete the batch.
                var cresult = await CompleteAsync(result.Batch!.Id, tracking, cancellationToken).ConfigureAwait(false);
                cresult.ExecuteStatus = result.ExecuteStatus;
                return cresult;
            }
            catch (Exception ex)
            {
                var eresult = ExceptionHandler(result, ex);
                if (eresult is not null)
                    return eresult;

                throw;
            }
        }

        /// <summary>
        /// Gets the current batch tracker information.
        /// </summary>
        private Task GetCurrentBatchTrackerAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            return EntityOrchestratorInvoker.Current.InvokeAsync(this, result, async (_, result, ct) =>
            {
                TBatchTracker? incompleteBatch = null;
                TBatchTracker? lastBatch = null;

                // Perform the initial sidecar execute to determine the current state.
                IMultiSetArgs[] multiSetArgs = [
                    new MultiSetCollArgs<List<TBatchTracker>, TBatchTracker>(_batchTrackerMapper, coll => incompleteBatch = coll.FirstOrDefault()),
                    new MultiSetSingleArgs<TBatchTracker>(_batchTrackerMapper, lb => lastBatch = lb, isMandatory: false)];

                await SidecarDatabase.StoredProcedure(ExecuteStoredProcedureName).SelectMultiSetAsync(multiSetArgs, ct).ConfigureAwait(false);

                result.Batch = incompleteBatch ?? lastBatch?.Adjust(x => x.Id = 0) ?? new TBatchTracker();
            }, cancellationToken);
        }

        /// <summary>
        /// Queries the CDC (Change Data Capture) for the batch and gets the resulting changes.
        /// </summary>
        private Task CdcBatchQueryAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            return EntityOrchestratorInvoker.Current.InvokeAsync(this, result, async (_, result, ct) =>
            {
                await GetBatchEntityDataAsync(result, ct).ConfigureAwait(false);
                result.ExecuteStatus = new EntityOrchestratorExecuteStatus { InitialCount = result.Result.Count };
            }, cancellationToken);
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <paramref name="multiSetArgs"/>; whilst also outputing the resulting return value.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <param name="batchTrackerLsnSynchronizer">The <see cref="BatchTracker"/> entity LSN synchronizer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The resultant <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</returns>
        protected async Task SelectQueryMultiSetAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, IEnumerable<IMultiSetArgs> multiSetArgs, Action<DatabaseRecord, TBatchTracker> batchTrackerLsnSynchronizer, CancellationToken cancellationToken = default)
        {
            var cmd = Database.SqlFromResource<TBatchTracker>(ExecuteCdcSqlResourceName)
                .Param(MaxQuerySizeParamName, MaxQuerySize)
                .Param(ContinueWithDataLossParamName, ContinueWithDataLoss);

            var tempParameters = new DatabaseParameterCollection(Database);
            _batchTrackerMapper.MapToDb(result.Batch, tempParameters);
            tempParameters.Where(p => p.ParameterName == "@BatchTrackingId" || p.ParameterName.EndsWith("Lsn")).ForEach(cmd.Parameters.Add);

            var msbt = new MultiSetBatchTracker(dr => batchTrackerLsnSynchronizer(dr, (TBatchTracker)result.Batch!));

            await cmd.SelectMultiSetAsync([msbt, .. multiSetArgs], cancellationToken).ConfigureAwait(false);

            result.Batch!.HasDataLoss = msbt.HasDataLoss;
            if (!(result.Batch.Id != 0 || msbt.HasChanges))
                result.Batch = null;
        }

        /// <summary>
        /// Gets the batch entity data from the database.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <typeparamref name="TEntityEnvelopeColl"/>.</returns>
        protected abstract Task GetBatchEntityDataAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current version tracking for the <paramref name="result"/>..
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="parameters">An optional <see cref="DatabaseParameterCollection"/> action.</param>
        /// <param name="multiSetArgs">An optional <see cref="IMultiSetArgs"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected internal async Task GetVersionTrackingAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, Action<DatabaseParameterCollection>? parameters, IMultiSetArgs? multiSetArgs, CancellationToken cancellationToken)
        {
            var dict = result.Result.ToDictionary(k => k is IGlobalIdentifier gi ? gi.TableKey.ToString()! : k.PrimaryKey.ToString()!, v => v);

            var cmd = SidecarDatabase.StoredProcedure(TrackingStoredProcedureName).JsonParam(VersionTrackingListParamName, dict.Keys);
            _batchTrackerMapper.MapToDb(result.Batch, cmd.Parameters);
            parameters?.Invoke(cmd.Parameters);

            var msa = MultiSetArgs.Create(
                new MultiSetSingleArgs<BatchTracker>(BatchTrackerMapper.Default, batchTracker =>
                {
                    // Update properties from the returned result set.
                    result.Batch!.Id = batchTracker.Id;
                    result.Batch.CreatedDate = batchTracker.CreatedDate;
                    result.Batch.CorrelationId = batchTracker.CorrelationId;
                }),
                new MultiSetCollArgs<List<VersionTracker>, VersionTracker>(new VersionTrackingMapper(), vtc => vtc.ForEach(vt => dict[vt.Key!].DatabaseTrackingHash = vt.Hash)));

            if (multiSetArgs is not null)
                msa = [.. msa, multiSetArgs];

            await cmd.SelectMultiSetAsync(msa, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Special-case MultiSetSingleArgs for the BatchTracker.
        /// </summary>
        private class MultiSetBatchTracker(Action<DatabaseRecord> action) : MultiSetSingleArgs
        {
            public bool HasDataLoss { get; private set; }

            public bool HasChanges { get; private set; }

            public override void DatasetRecord(DatabaseRecord dr)
            {
                HasDataLoss = dr.GetValue<bool>(nameof(HasDataLoss));
                HasChanges = dr.GetValue<bool>(nameof(HasChanges));
                action(dr);
            }
        }
    }
}