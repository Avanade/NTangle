// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace NTangle.Cdc
{
    /// <summary>
    /// Manages the Change Data Capture (CDC) entity (aggregate root) sidecar-based orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TBatchTracker">The entity <see cref="BatchTracker"/>.</typeparam>
    /// <typeparam name="TBatchTrackerMapper">The entity <see cref="BatchTracker"/> mapper.</typeparam>
    public abstract class EntitySidecarOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TBatchTracker, TBatchTrackerMapper> 
        : EntitySidecarOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TBatchTracker, TBatchTrackerMapper>, IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TBatchTracker : BatchTracker, new()
        where TBatchTrackerMapper : class, IDatabaseMapperEx<TBatchTracker>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestrator{TEntity, TEntityEnvelopeColl, TEntityEnvelope}"/> class.
        /// </summary>
        /// <param name="database">The primary <see cref="IDatabase"/>.</param>
        /// <param name="sidecarDatabase">The sidecar <see cref="ISidecarDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public EntitySidecarOrchestrator(IDatabase database, ISidecarDatabase sidecarDatabase, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger)
            : base(database, sidecarDatabase, eventPublisher, jsonSerializer, settings, logger) => PostConsolidationProcessingAsync = PersistTrackingAndGetVersionAsync;

        /// <summary>
        /// Creates/Updates tracking, gets current versions.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        private Task PersistTrackingAndGetVersionAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken = default)
            => GetVersionTrackingAsync(result, null, null, cancellationToken);
    }
}