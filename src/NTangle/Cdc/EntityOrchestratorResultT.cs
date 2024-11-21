// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using System;
using System.Collections.Generic;

namespace NTangle.Cdc
{
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
        public TEntityEnvelopeColl Result { get; private set; } = new TEntityEnvelopeColl();

        /// <summary>
        /// Sets (overrides) the <see cref="Result"/>.
        /// </summary>
        /// <param name="result">The new <see cref="Result"/>.</param>
        internal void SetResult(TEntityEnvelopeColl result) => Result = result.ThrowIfNull(nameof(result));
    }
}