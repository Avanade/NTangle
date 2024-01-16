// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using System;
using System.Collections.Generic;

namespace NTangle.Cdc
{
    /// <summary>
    /// Manages the Change Data Capture (CDC) entity (aggregate root) orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TVersionTrackerMapper">The <see cref="VersionTracker"/> database and table-valued parameter mapper <see cref="Type"/>.</typeparam>
    /// <param name="db">The <see cref="IDatabase"/>.</param>
    /// <param name="executeStoredProcedureName">The name of the batch execute stored procedure.</param>
    /// <param name="completeStoredProcedureName">The name of the batch complete stored procedure.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public abstract class EntityOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper>(IDatabase db, string executeStoredProcedureName, string completeStoredProcedureName, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger) : EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper>(db, executeStoredProcedureName, completeStoredProcedureName, eventPublisher, jsonSerializer, settings, logger), IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TVersionTrackerMapper : IDatabaseMapper<VersionTracker>, IDatabaseTvp<VersionTracker>, new()
    { }
}