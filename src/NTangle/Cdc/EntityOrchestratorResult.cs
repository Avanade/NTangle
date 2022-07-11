// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Collections.Generic;

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> result.
    /// </summary>
    public class EntityOrchestratorResult
    {
        /// <summary>
        /// Gets the internally allocated execution identifier.
        /// </summary>
        public Guid ExecutionId { get; internal set; }

        /// <summary>
        /// Gets the <see cref="BatchTracker"/> where <see cref="IsSuccessful"/>.
        /// </summary>
        public BatchTracker? Batch { get; internal set; }

        /// <summary>
        /// Indicates that the outbox execution is successful and can continue.
        /// </summary>
        public bool IsSuccessful => Exception == null;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> where <b>not</b> <see cref="IsSuccessful"/>
        /// </summary>
        public Exception? Exception { get; internal set; }

        /// <summary>
        /// Gets the <see cref="EntityOrchestratorExecuteStatus"/>.
        /// </summary>
        /// <remarks>This will only be updated as a result of <see cref="IEntityOrchestrator.ExecuteAsync(System.Threading.CancellationToken)"/>.</remarks>
        public EntityOrchestratorExecuteStatus? ExecuteStatus { get; internal set; }
    }

    /// <summary>
    /// Provides the specific status for the <see cref="IEntityOrchestrator.ExecuteAsync(System.Threading.CancellationToken)"/> method.
    /// </summary>
    public class EntityOrchestratorExecuteStatus
    {
        /// <summary>
        /// Gets the initial count of entity operations returned from the initial database execution; i.e. as retrieved directly from the database change-data-capture logs.
        /// </summary>
        /// <remarks>There could be multiple operations per entity; i.e. create, update, delete, etc. that will in turn be consolidated.</remarks>
        public int? InitialCount { get; internal set; }

        /// <summary>
        /// Gets the count of entities after the operations have been consolidated. 
        /// </summary>
        /// <remarks>For example, a) multiple updates become a single update; b) a create and multiple updates become a single create; c) multiple updates and a delete become a single delete; and d) a create, zero or more updates, and delete within single batch (i.e. in quick succession) are ignored (skipped).</remarks>
        public int? ConsolidatedCount { get; internal set; }

        /// <summary>
        /// Gets the count of entities to be published after each entity JSON content has been verified as unique (see <see cref="VersionTracker"/>).
        /// </summary>
        public int? PublishCount { get; internal set; }
    }

    /// <summary>
    /// Represents the typed-<see cref="EntityOrchestratorResult"/> result.
    /// </summary>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The entity envelope <see cref="Type"/>.</typeparam>
    public sealed class EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> : EntityOrchestratorResult where TEntityEnvelopeColl : List<TEntityEnvelope>, new() where TEntityEnvelope : class, IEntityEnvelope
    {
        /// <summary>
        /// Gets the resulting <typeparamref name="TEntityEnvelopeColl"/>.
        /// </summary>
        public TEntityEnvelopeColl Result { get; } = new TEntityEnvelopeColl();
    }
}