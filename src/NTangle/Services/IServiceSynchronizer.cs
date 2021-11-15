// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Cdc;
using System;

namespace NTangle.Services
{
    /// <summary>
    /// To ensure that only a single <see cref="HostedService"/> is performing the Change Data Capture (CDC) processing for an <see cref="IEntity"/> this needs to be synchronized as the underlying
    /// <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> does not support execution concurrency.
    /// </summary>
    /// <remarks>The <see cref="Enter{T}"/> must acquire and hold the lock until the corresponding <see cref="Exit"/> is invoked. Where a lock is unable to be acquired then a <c>false</c> must be returned to advise the caller
    /// that processing can not occur at this time as another process is currently executing. A result of <c>true</c> indicates the lock was acquired and will be held until the corresponding <see cref="Exit"/>.</remarks>
    public interface IServiceSynchronizer : IDisposable
    {
        /// <summary>
        /// Acquires a lock.
        /// </summary>
        /// <returns><c>true</c> if the lock is aquired; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The <see cref="Type"/> to lock.</typeparam>
        bool Enter<T>();

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to unlock.</typeparam>
        void Exit<T>();
    }
}