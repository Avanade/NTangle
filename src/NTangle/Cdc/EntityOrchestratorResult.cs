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
        public bool IsSuccessful => Exception  == null;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> where <b>not</b> <see cref="IsSuccessful"/>
        /// </summary>
        public Exception? Exception { get; internal set; }
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