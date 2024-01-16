// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Configuration;
using CoreEx.Database;
using CoreEx.Database.SqlServer;
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
    /// Manages the Change Data Capture (CDC) entity (aggregate root) orchestration capability where also using global identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The root entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelopeColl">The <typeparamref name="TEntityEnvelope"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntityEnvelope">The <typeparamref name="TEntity"/> envelope <see cref="Type"/>.</typeparam>
    /// <typeparam name="TVersionTrackerMapper">The <see cref="VersionTracker"/> database and table-valued parameter mapper <see cref="Type"/>.</typeparam>
    /// <typeparam name="TGlobalIdentifer">The global identifier <see cref="Type"/>.</typeparam>
    public abstract class EntityOrchestrator<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper, TGlobalIdentifer> : EntityOrchestratorBase<TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper>, IEntityOrchestrator<TEntity>
        where TEntity : class, IEntity, new()
        where TEntityEnvelopeColl : List<TEntityEnvelope>, new()
        where TEntityEnvelope : class, TEntity, IEntityEnvelope, new()
        where TVersionTrackerMapper : IDatabaseMapper<VersionTracker>, IDatabaseTvp<VersionTracker>, new()
    { 
        private const string IdentifierListParamName = "IdentifierList";

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestrator{TEntity, TEntityEnvelopeColl, TEntityEnvelope, TVersionTrackerMapper, TGlobalIdentifer}"/> class that requires identifier mapping support.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="executeStoredProcedureName">The name of the batch execute stored procedure.</param>
        /// <param name="completeStoredProcedureName">The name of the batch complete stored procedure.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="identifierMappingStoredProcedureName">The name of the optional identifier mapping stored procedure.</param>
        /// <param name="identifierGenerator">The <see cref="IIdentifierGenerator{TCdcIdentifer}"/>.</param>
        /// <param name="identifierMappingMapper">The <see cref="IdentifierMappingMapperBase{TCdcIdentifer}"/>.</param>
        public EntityOrchestrator(IDatabase db, string executeStoredProcedureName, string completeStoredProcedureName, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger logger, string identifierMappingStoredProcedureName, IIdentifierGenerator<TGlobalIdentifer> identifierGenerator, IdentifierMappingMapperBase<TGlobalIdentifer> identifierMappingMapper)
            : base(db, executeStoredProcedureName, completeStoredProcedureName, eventPublisher, jsonSerializer, settings, logger)
        {
            IdentifierMappingStoredProcedureName = identifierMappingStoredProcedureName.ThrowIfNull(nameof(identifierMappingStoredProcedureName));
            IdentifierGenerator = identifierGenerator.ThrowIfNull(nameof(identifierGenerator));
            IdentifierMappingMapper = identifierMappingMapper.ThrowIfNull(nameof(identifierMappingMapper));
            AdditionalEnvelopeProcessingAsync = AssignIdentityMappingAsync;
        }

        /// <summary>
        /// Gets the name of the <b>identifier mapping</b> stored procedure.
        /// </summary>
        public string IdentifierMappingStoredProcedureName { get; }

        /// <summary>
        /// Gets the <see cref="IIdentifierGenerator{TCdcIdentifer}"/>.
        /// </summary>
        public IIdentifierGenerator<TGlobalIdentifer> IdentifierGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IdentifierMappingMapperBase{TCdcIdentifer}"/>.
        /// </summary>
        public IdentifierMappingMapperBase<TGlobalIdentifer> IdentifierMappingMapper { get; }

        /// <summary>
        /// Assigns the identity mapping by adding <i>new</i> for those items that do not currentyly have a global identifier currently assigned.
        /// </summary>
        /// <param name="coll">The entity envelope collection.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        protected async Task AssignIdentityMappingAsync(TEntityEnvelopeColl coll, CancellationToken cancellationToken = default)
        {
            // Find all the instances where there is currently no global identifier assigned.
            var vimc = new ValueIdentifierMappingCollection<TGlobalIdentifer>();
            await coll.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEachAsync(async item => await item.LinkIdentifierMappingsAsync(vimc, IdentifierGenerator!).ConfigureAwait(false)).ConfigureAwait(false);
            if (vimc.Count == 0)
                return;

            // There could be multiple references to same Schema/Table/Key; these need to filtered out; i.e. send only a distinct list.
            var imcd = new Dictionary<(string?, string?, string?), IdentifierMapping<TGlobalIdentifer>>();
            vimc.ForEach(item => imcd.TryAdd((item.Schema, item.Table, item.Key), item));
            var tvp = IdentifierMappingMapper!.CreateTableValuedParameter(imcd.Values);

            // Execute the stored procedure and get the updated list.
            var imc = await Db.StoredProcedure(IdentifierMappingStoredProcedureName!).Params(p => p.AddTableValuedParameter(IdentifierListParamName, tvp)).SelectQueryAsync(IdentifierMappingMapper, cancellationToken).ConfigureAwait(false);
            if (imc.Count() != imcd.Count)
                throw new InvalidOperationException($"Stored procedure '{IdentifierMappingStoredProcedureName}' returned an unexpected result.");

            // Re-link the identifier mappings with the final value.
            vimc.ForEach(item => item.GlobalId = imc.Single(x => x.Schema == item.Schema && x.Table == item.Table && x.Key == item.Key).GlobalId);
            coll.OfType<ILinkIdentifierMapping<TGlobalIdentifer>>().ForEach(item => item.RelinkIdentifierMappings(vimc));
        }
    }
}