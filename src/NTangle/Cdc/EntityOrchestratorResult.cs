// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

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
}