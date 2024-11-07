// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Json;
using CoreEx.Results;
using Microsoft.Extensions.Logging;
using NTangle.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Manages the Change Data Capture (CDC) entity (aggregate root) orchestration capability where also using global identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TGlobalIdentifer">The global identifier <see cref="Type"/>.</typeparam>
    public abstract class EntityOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TGlobalIdentifer> : EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope>, IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
    { 
        private const string IdentifierListParamName = "IdentifierList";

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestrator{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TGlobalIdentifer}"/> class that requires identifier mapping support.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="identifierGenerator">The <see cref="IIdentifierGenerator{TCdcIdentifer}"/>.</param>
        public EntityOrchestrator(IDatabase database, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger, IIdentifierGenerator<TGlobalIdentifer> identifierGenerator)
            : base(database, eventPublisher, jsonSerializer, settings, logger)
        {
            IdentifierGenerator = identifierGenerator.ThrowIfNull(nameof(identifierGenerator));
            PostConsolidationProcessingAsync = AssignIdentityMappingAsync;
        }

        /// <summary>
        /// Gets the name of the <b>identifier mapping</b> stored procedure.
        /// </summary>
        protected abstract string IdentifierMappingStoredProcedureName { get; }

        /// <summary>
        /// Gets the <see cref="IIdentifierGenerator{TCdcIdentifer}"/>.
        /// </summary>
        protected IIdentifierGenerator<TGlobalIdentifer> IdentifierGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IdentifierMappingMapper{TCdcIdentifer}"/>.
        /// </summary>
        protected IdentifierMappingMapper<TGlobalIdentifer> IdentifierMappingMapper { get; } = new IdentifierMappingMapper<TGlobalIdentifer>();

        /// <summary>
        /// Assigns the identity mapping by adding <i>new</i> for those items that do not currentyly have a global identifier currently assigned.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected async Task AssignIdentityMappingAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken = default)
        {
            // Find all the instances where there is currently no global identifier assigned.
            var vimc = new ValueIdentifierMappingCollection<TGlobalIdentifer>();
            await result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEachAsync(async item => await item.LinkIdentifierMappingsAsync(vimc, IdentifierGenerator!).ConfigureAwait(false)).ConfigureAwait(false);
            if (vimc.Count == 0)
                return;

            // There could be multiple references to same Schema/Table/Key; these need to filtered out; i.e. send only a distinct list.
            var imcd = new Dictionary<(string?, string?, string?), IdentifierMapping<TGlobalIdentifer>>();
            vimc.ForEach(item => imcd.TryAdd((item.Schema, item.Table, item.Key), item));

            // Execute the stored procedure and get the updated list.
            var imc = await Database.StoredProcedure(IdentifierMappingStoredProcedureName!).Params(p => p.AddJsonParameter(IdentifierListParamName, imcd.Values)).SelectQueryAsync(IdentifierMappingMapper, cancellationToken).ConfigureAwait(false);
            if (imc.Count() != imcd.Count)
                throw new InvalidOperationException($"Stored procedure '{IdentifierMappingStoredProcedureName}' returned an unexpected result.");

            // Re-link the identifier mappings with the final value.
            vimc.ForEach(item => item.GlobalId = imc.Single(x => x.Schema == item.Schema && x.Table == item.Table && x.Key == item.Key).GlobalId);
            result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEach(item => item.RelinkIdentifierMappings(vimc));
        }
    }
}