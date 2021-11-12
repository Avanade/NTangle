// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Cdc;
using System;

namespace NTangle.Services
{
    /// <summary>
    /// To ensure that only a single <see cref="HostedService"/> is performing the Change Data Capture (CDC) processing for an <see cref="IEntity"/> this needs to be synchronized as the underlying
    /// <see cref="EntityOrchestratorBase{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper}"/> does not support execution concurrency.
    /// </summary>
    public interface IHostedServiceSynchronizer : IDisposable
    {
        /// <summary>
        /// Acquires a lock.
        /// </summary>
        /// <returns><c>true</c> if the lock is aquired; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The <see cref="IEntity"/> <see cref="Type"/>.</typeparam>
        bool Enter<T>() where T : IEntity;

        /// <summary>
        /// Releases the lock.
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntity"/> <see cref="Type"/>.</typeparam>
        void Exit<T>() where T : IEntity;
    }
}