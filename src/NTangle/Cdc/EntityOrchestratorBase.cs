// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Events;
using CoreEx.Json;
using CoreEx.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope> : EntityOrchestratorCore<TEntity, TEntityEnvelopeColl, TEntityEnvelope>, IEntityOrchestrator
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope}"/> class with no identifier mapping support.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        internal EntityOrchestratorBase(IDatabase database, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger)
            : base(eventPublisher, jsonSerializer, settings, logger) => Database = database.ThrowIfNull(nameof(database));

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; }

        /// <summary>
        /// Gets the name of the batch <b>execute</b> stored procedure.
        /// </summary>
        protected abstract string ExecuteStoredProcedureName { get; }

        /// <summary>
        /// Gets the name of the batch <b>complete</b> stored procedure.
        /// </summary>
        protected abstract string CompleteStoredProcedureName { get; }

        /// <inheritdoc/>
        public override Task<EntityOrchestratorResult> CompleteAsync(long batchTrackerId, List<VersionTracker> versionTracking, CancellationToken cancellationToken = default)
            => CompleteAsync(Database, CompleteStoredProcedureName, batchTrackerId, versionTracking, cancellationToken);

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
        private async Task<EntityOrchestratorResult> ExecuteInternalAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the requested CDC batch data.
                LogQueryStart();
                var stopwatch = Stopwatch.StartNew();

                await CdcBatchQueryAsync(result, cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                LogQueryComplete(result, stopwatch);
                if (result.BatchTracker == null)
                    return result;

                // Consolidate the results.
                await ConsolidateAsync(result, cancellationToken).ConfigureAwait(false);

                // Post-consolidation processing.
                await PostConsolidationAsync(result, cancellationToken).ConfigureAwait(false);

                // Additional processing.
                await AdditionalAsync(result, cancellationToken).ConfigureAwait(false);

                // Determine whether anything may have been sent before (version tracking) and exclude (i.e. do not send again).
                var tracking = await VersionAsync(result, cancellationToken).ConfigureAwait(false);

                // Publish & send the events.
                await PublishAndSendEventsAsync(result, cancellationToken).ConfigureAwait(false);

                // Complete the batch.
                var cresult = await CompleteAsync(result.BatchTracker.Id, tracking, cancellationToken).ConfigureAwait(false);
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
        /// Queries the CDC (Change Data Capture) for the batch and gets the resulting changes.
        /// </summary>
        private Task CdcBatchQueryAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken)
        {
            return EntityOrchestratorInvoker.Current.InvokeAsync(this, result, async (_, result, ct) =>
            {
                await GetBatchEntityDataAsync(result, cancellationToken).ConfigureAwait(false);
                result.ExecuteStatus = new EntityOrchestratorExecuteStatus { InitialCount = result.Result.Count };
            }, cancellationToken);
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <paramref name="multiSetArgs"/>; whilst also outputing the resulting return value.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected async Task SelectQueryMultiSetAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, IEnumerable<IMultiSetArgs> multiSetArgs, CancellationToken cancellationToken = default)
        {
            var msa = new List<IMultiSetArgs> { new MultiSetSingleArgs<BatchTracker>(BatchTrackerMapper, r => result.BatchTracker = r, isMandatory: false, stopOnNull: true) };
            msa.AddRange(multiSetArgs);

            await Database.StoredProcedure(ExecuteStoredProcedureName).Params(p =>
            {
                p.AddParameter(MaxQuerySizeParamName, MaxQuerySize);
                p.AddParameter(ContinueWithDataLossParamName, ContinueWithDataLoss);
            }).SelectMultiSetAsync(msa, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the batch entity data from the database.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <typeparamref name="TEntityEnvelopeColl"/>.</returns>
        protected abstract Task GetBatchEntityDataAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken);
    }
}