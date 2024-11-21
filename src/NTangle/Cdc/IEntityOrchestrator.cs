// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Enables the core Change Data Capture (CDC) entity (aggregate root) orchestration capability.
    /// </summary>
    public interface IEntityOrchestrator
    {
        /// <summary>
        /// Gets the unique identifier for the current execution.
        /// </summary>
        Guid ExecutionId { get; }

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="BatchTracker"/>.
        /// </summary>
        int MaxQuerySize { get; set; }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur on the likes of SQL Server see <see href="https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server"/>.</remarks>
        bool ContinueWithDataLoss { get; set; }

        /// <summary>
        /// Executes the next (new) outbox, or reprocesses the last incomplete, then <see cref="CompleteAsync">completes</see> on success.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        /// <remarks>An outbox may be incomplete where there was a previous execution failure.</remarks>
        Task<EntityOrchestratorResult> ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes an existing outbox updating the corresponding <paramref name="tracking"/> where appropriate.
        /// </summary>
        /// <param name="outboxId">The outbox identifer.</param>
        /// <param name="tracking">The <see cref="VersionTracker"/> list.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="EntityOrchestratorResult"/>.</returns>
        Task<EntityOrchestratorResult> CompleteAsync(long outboxId, List<VersionTracker> tracking, CancellationToken cancellationToken = default);
    }
}