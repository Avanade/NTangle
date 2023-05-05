// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
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
}