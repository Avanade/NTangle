// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Json;
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
    /// Manages the Change Data Capture (CDC) entity (aggregate root) sidecar-based orchestration capability where also using global identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TGlobalIdentifer">The global identifier <see cref="Type"/>.</typeparam>
    /// <typeparam name="TBatchTracker">The entity <see cref="BatchTracker"/>.</typeparam>
    /// <typeparam name="TBatchTrackerMapper">The entity <see cref="BatchTracker"/> mapper.</typeparam>
    public abstract class EntitySidecarOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TBatchTracker, TBatchTrackerMapper, TGlobalIdentifer> 
        : EntitySidecarOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TBatchTracker, TBatchTrackerMapper>, IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TBatchTracker : BatchTracker, new()
        where TBatchTrackerMapper : class, IDatabaseMapperEx<TBatchTracker>, new()
    {
        private const string IdentifierListParamName = "IdentifierList";

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestrator{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TGlobalIdentifer}"/> class that requires identifier mapping support.
        /// </summary>
        /// <param name="database">The primary <see cref="IDatabase"/>.</param>
        /// <param name="sidecarDatabase">The sidecar <see cref="ISidecarDatabase"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="identifierGenerator">The <see cref="IIdentifierGenerator{TCdcIdentifer}"/>.</param>
        public EntitySidecarOrchestrator(IDatabase database, ISidecarDatabase sidecarDatabase, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger, IIdentifierGenerator<TGlobalIdentifer> identifierGenerator)
            : base(database, sidecarDatabase, eventPublisher, jsonSerializer, settings, logger)
        {
            IdentifierGenerator = identifierGenerator.ThrowIfNull(nameof(identifierGenerator));
            IdentifierMappingMapper = new IdentifierMappingMapper<TGlobalIdentifer>();
            PostConsolidationProcessingAsync = PersistTrackingAndGetVersionAsync;
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
        protected IdentifierMappingMapper<TGlobalIdentifer> IdentifierMappingMapper { get; }

        /// <summary>
        /// Creates/Updates tracking, gets current versions, and assigns the identity mapping by adding <i>new</i> for those items that do not currentyly have a global identifier currently assigned.
        /// </summary>
        /// <param name="result">The <see cref="EntityOrchestratorResult{TEntityEnvelopeColl, TEntityEnvelope}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        private async Task PersistTrackingAndGetVersionAsync(EntityOrchestratorResult<TEntityEnvelopeColl, TEntityEnvelope> result, CancellationToken cancellationToken = default)
        {
            // ** Stage 1 ** - Get the current mappings and assign before we attempt to create any new.
            var vimc = new ValueIdentifierMappingCollection<TGlobalIdentifer>();
            result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEach(item => item.GetIdentifierMappingConfigurations(vimc));

            // There could be multiple references to same Schema/Table/Key; these need to filtered out; i.e. send only a distinct list.
            var imcd = new Dictionary<(string? Schema, string? Table, string? Key), IdentifierMapping<TGlobalIdentifer>>();
            vimc.ForEach(item => imcd.TryAdd((item.Schema, item.Table, item.Key), item));

            var msa = new MultiSetCollArgs<List<IdentifierMapping<TGlobalIdentifer>>, IdentifierMapping<TGlobalIdentifer>>(IdentifierMappingMapper, mappings =>
            {
                // Re-link the identifier mappings with the final value.
                vimc.ForEach(item => item.GlobalId = mappings.Single(x => x.Schema == item.Schema && x.Table == item.Table && x.Key == item.Key).GlobalId);
                result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEach(item => item.RelinkIdentifierMappings(vimc));
            });

            // Note: This method also supports the getting of current mappings for the specified Schema/Table/Key as per passed 'MultiSetCollArgs'; as well as, getting the current Hash/ETag where applicable.
            await GetVersionTrackingAsync(result, p => p.AddJsonParameter(IdentifierListParamName, imcd.Keys.Select(k => new { k.Schema, k.Table, k.Key })), msa, cancellationToken).ConfigureAwait(false);

            // ** Stage 2 ** - Assign new identifier where there is currently no global identifier assigned.
            vimc.Clear();
            await result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEachAsync(async item => await item.LinkIdentifierMappingsAsync(vimc, IdentifierGenerator!).ConfigureAwait(false)).ConfigureAwait(false);
            if (vimc.Count == 0)
                return;

            // There could be multiple references to same Schema/Table/Key; these need to filtered out; i.e. send only a distinct list.
            imcd.Clear();
            vimc.ForEach(item => imcd.TryAdd((item.Schema, item.Table, item.Key), item));

            // Execute the stored procedure and get the updated list.
            var imc = await SidecarDatabase.StoredProcedure(IdentifierMappingStoredProcedureName).JsonParam(IdentifierListParamName, imcd.Values).SelectQueryAsync(IdentifierMappingMapper, cancellationToken).ConfigureAwait(false);
            if (imc.Count() != imcd.Count)
                throw new InvalidOperationException($"Stored procedure '{IdentifierMappingStoredProcedureName}' returned an unexpected result.");

            // Re-link the identifier mappings with the final value.
            vimc.ForEach(item => item.GlobalId = imc.Single(x => x.Schema == item.Schema && x.Table == item.Table && x.Key == item.Key).GlobalId);
            result.Result.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEach(item => item.RelinkIdentifierMappings(vimc));
        }
    }
}