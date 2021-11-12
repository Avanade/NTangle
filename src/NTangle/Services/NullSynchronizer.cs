// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Cdc;

namespace NTangle.Services
{
    /// <summary>
    /// An <see cref="IHostedServiceSynchronizer"/> that performs <b>no</b> synchronization in that <see cref="Enter"/> will always return <c>true</c>.
    /// </summary>
    /// <remarks>This should be used in scenarios where synchronization is not required as this is handled externally. If this is <i>not</i> guaranteed then the consistency of the underlying
    /// <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> processing equally cannot be guaranteed.</remarks>
    public sealed class NullSynchronizer : IHostedServiceSynchronizer
    {
        /// <inheritdoc/>
        public bool Enter<T>() where T : IEntity => true;

        /// <inheritdoc/>
        public void Exit<T>() where T : IEntity { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}