/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Microsoft.Extensions.Logging;
using NTangle;
using NTangle.Cdc;
using NTangle.Data;
using NTangle.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlServerDemo.Publisher.Entities;

namespace SqlServerDemo.Publisher.Data
{
    /// <summary>
    /// Enables the Change Data Capture (CDC) <see cref="ContactCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Contact]').
    /// </summary>
    public partial interface IContactCdcOrchestrator : IEntityOrchestrator<ContactCdc> { }

    /// <summary>
    /// Manages the Change Data Capture (CDC) <see cref="ContactCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Contact]').
    /// </summary>
    public partial class ContactCdcOrchestrator : EntityOrchestrator<ContactCdc, ContactCdcOrchestrator.ContactCdcEnvelopeCollection, ContactCdcOrchestrator.ContactCdcEnvelope, VersionTrackingMapper, string>, IContactCdcOrchestrator
    {
        private static readonly ContactCdcMapper _contactCdcMapper = new ContactCdcMapper();
        private static readonly AddressCdcMapper _addressCdcMapper = new AddressCdcMapper();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCdcOrchestrator"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="idGen">The <see cref="IIdentifierGenerator{T}"/>.</param>
        public ContactCdcOrchestrator(IDatabase db, IEventPublisher evtPub, ILogger<ContactCdcOrchestrator> logger, IIdentifierGenerator<string> idGen) :
            base(db, "[NTangle].[spContactBatchExecute]", "[NTangle].[spContactBatchComplete]", evtPub, logger, "[NTangle].[spIdentifierMappingCreate]", idGen, new IdentifierMappingMapper<string>()) => ContactCdcOrchestratorCtor();

        partial void ContactCdcOrchestratorCtor(); // Enables additional functionality to be added to the constructor.

        /// <inheritdoc/>
        protected override async Task<EntityOrchestratorResult<ContactCdcEnvelopeCollection, ContactCdcEnvelope>> GetBatchEntityDataAsync()
        {
            var cColl = new ContactCdcEnvelopeCollection();

            var result = await SelectQueryMultiSetAsync(
                // Root table: '[Legacy].[Contact]'
                new MultiSetCollArgs<ContactCdcEnvelopeCollection, ContactCdcEnvelope>(_contactCdcMapper, r => cColl = r, stopOnNull: true),

                // Join table: '[Legacy].[Address]' (unique name 'Address')
                new MultiSetCollArgs<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>(_addressCdcMapper, r =>
                {
                    foreach (var a in r.GroupBy(x => new { x.AddressId }).Select(g => new { g.Key.AddressId, Coll = g.ToCollection<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>() }))
                    {
                        cColl.Where(x => x.AddressId == a.AddressId).ForEach(x => x.Address = a.Coll.FirstOrDefault());
                    }
                })).ConfigureAwait(false);

            result.Result.AddRange(cColl);
            return result;
        }

        /// <inheritdoc/>
        protected override string EventSubject => "Legacy.Contact";

        /// <inheritdoc/>
        protected override EventSubjectFormat EventSubjectFormat => EventSubjectFormat.NameOnly;

        /// <inheritdoc/>
        protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

        /// <inheritdoc/>
        protected override Uri? EventSource => new Uri("/database/cdc/legacy/contact", UriKind.Relative);

        /// <inheritdoc/>
        protected override EventSourceFormat EventSourceFormat { get; } = EventSourceFormat.NameAndTableKey;

        /// <summary>
        /// Represents a <see cref="ContactCdc"/> envelope to append the required (additional) database properties.
        /// </summary>
        public class ContactCdcEnvelope : ContactCdc, IEntityEnvelope
        {
            /// <inheritdoc/>
            public OperationType DatabaseOperationType { get; set; }

            /// <inheritdoc/>
            public string? DatabaseTrackingHash { get; set; }

            /// <inheritdoc/>
            public byte[] DatabaseLsn { get; set; }
        }

        /// <summary>
        /// Represents a <see cref="ContactCdcEnvelope"/> collection.
        /// </summary>
        public class ContactCdcEnvelopeCollection : List<ContactCdcEnvelope> { }

        /// <summary>
        /// Represents a <see cref="ContactCdc"/> database mapper.
        /// </summary>
        public class ContactCdcMapper : IDatabaseMapper<ContactCdcEnvelope>
        {
            /// <inheritdoc/>
            public ContactCdcEnvelope MapFromDb(DatabaseRecord record) => new ContactCdcEnvelope
            {
                GlobalId = record.GetValue<string?>("GlobalId"),
                CID = record.GetValue<int>("CID"),
                Name = record.GetValue<string?>("Name"),
                Phone = record.GetValue<string?>("Phone"),
                Email = record.GetValue<string?>("Email"),
                Active = record.GetValue<bool?>("Active"),
                DontCallList = record.GetValue<bool?>("DontCallList"),
                AddressId = record.GetValue<int?>("AddressId"),
                AlternateContactId = record.GetValue<int?>("AlternateContactId"),
                GlobalAlternateContactId = record.GetValue<string?>("GlobalAlternateContactId"),
                UniqueId = record.GetValue<Guid>("UniqueId"),
                DatabaseOperationType = record.GetValue<OperationType>("_OperationType"),
                DatabaseTrackingHash = record.GetValue<string>("_TrackingHash"),
                DatabaseLsn = record.GetValue<byte[]>("_Lsn")
            };
        }

        /// <summary>
        /// Represents a <see cref="AddressCdc"/> database mapper.
        /// </summary>
        public class AddressCdcMapper : IDatabaseMapper<ContactCdc.AddressCdc>
        {
            /// <inheritdoc/>
            public ContactCdc.AddressCdc MapFromDb(DatabaseRecord record) => new ContactCdc.AddressCdc
            {
                AddressId = record.GetValue<int>("AddressId"),
                Street1 = record.GetValue<string?>("Street1"),
                Street2 = record.GetValue<string?>("Street2"),
                AlternateAddressId = record.GetValue<int?>("AlternateAddressId"),
                GlobalAlternateAddressId = record.GetValue<string?>("GlobalAlternateAddressId")
            };
        }
    }
}

#pragma warning restore
#nullable restore