// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Cdc;

namespace NTangle.Services
{
    /// <summary>
    /// An <see cref="IServiceSynchronizer"/> that performs <b>no</b> synchronization in that <see cref="Enter"/> will always return <c>true</c> resulting in concurrenct execution.
    /// </summary>
    /// <remarks>This should be used in scenarios where synchronization is not required as this is handled externally. If this is <i>not</i> guaranteed then the consistency of the underlying
    /// <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> processing equally cannot be guaranteed.</remarks>
    public sealed class ConcurrentSynchronizer : IServiceSynchronizer
    {
        /// <inheritdoc/>
        public bool Enter<T>(string? name = null) => true;

        /// <inheritdoc/>
        public void Exit<T>(string? name) { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}