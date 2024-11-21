// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Events;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
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
    /// <param name="database">The <see cref="IDatabase"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public abstract class EntityOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope>(IDatabase database, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger) 
        : EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope>(database, eventPublisher, jsonSerializer, settings, logger), IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new() { }
}