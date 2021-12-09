/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Newtonsoft.Json;
using NTangle;
using NTangle.Cdc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlServerDemo.Publisher.Entities
{
    /// <summary>
    /// Represents the CDC model for the root (parent) database table '[Legacy].[Contact]'.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class ContactCdc : IEntity, IPartitionKey, IGlobalIdentifier<string>, ILinkIdentifierMapping<string>
    {
        /// <inheritdoc/>
        [JsonProperty("globalId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? GlobalId { get; set; }

        /// <summary>
        /// Gets or sets the CID '[Legacy].[Contact].[ContactId]' column value.
        /// </summary>
        public int CID { get; set; }

        /// <summary>
        /// Gets or sets the Name '[Legacy].[Contact].[Name]' column value.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the Phone '[Legacy].[Contact].[Phone]' column value.
        /// </summary>
        [JsonProperty("phone", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the Email '[Legacy].[Contact].[Email]' column value.
        /// </summary>
        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the Active '[Legacy].[Contact].[Active]' column value.
        /// </summary>
        [JsonProperty("active", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or sets the Dont Call List '[Legacy].[Contact].[DontCallList]' column value.
        /// </summary>
        [JsonProperty("dontCallList", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? DontCallList { get; set; }

        /// <summary>
        /// Gets or sets the Address Id '[Legacy].[Contact].[AddressId]' column value.
        /// </summary>
        [JsonProperty("addressId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? AddressId { get; set; }

        /// <summary>
        /// Gets or sets the Alternate Contact Id '[Legacy].[Contact].[AlternateContactId]' column value.
        /// </summary>
        public int? AlternateContactId { get; set; }

        /// <summary>
        /// Gets or sets the Global Alternate Contact Id '[Legacy].[Contact].[AlternateContactId]' mapped identifier value.
        /// </summary>
        [JsonProperty("globalAlternateContactId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? GlobalAlternateContactId { get; set; }

        /// <summary>
        /// Gets or sets the Unique Id column value (left join table '[Legacy].[ContactMapping].[UniqueId]').
        /// </summary>
        [JsonProperty("uniqueId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the related (one-to-one) <see cref="ContactCdc.Address"/> (database table '[Legacy].[Address]').
        /// </summary>
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ContactCdc.AddressCdc? Address { get; set; }

        /// <inheritdoc/>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <inheritdoc/>
        public CompositeKey PrimaryKey => new CompositeKey(GlobalId);

        /// <inheritdoc/>
        public string? PartitionKey => "Contact";

        /// <inheritdoc/>
        public CompositeKey TableKey => new CompositeKey(CID);

        /// <inheritdoc/>
        public async Task LinkIdentifierMappingsAsync(ValueIdentifierMappingCollection<string> coll, IIdentifierGenerator<string> idGen)
        {
            coll.AddAsync(GlobalId == default, async () => new ValueIdentifierMapping<string> { Value = this, Property = nameof(GlobalId), Schema = "Legacy", Table = "Contact", Key = TableKey.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<ContactCdc>().ConfigureAwait(false) });
            coll.AddAsync(GlobalAlternateContactId == default && AlternateContactId != default, async () => new ValueIdentifierMapping<string> { Value = this, Property = nameof(GlobalAlternateContactId), Schema = "Legacy", Table = "Contact", Key = AlternateContactId.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<ContactCdc>().ConfigureAwait(false) });
            await (Address?.LinkIdentifierMappingsAsync(coll, idGen) ?? Task.CompletedTask).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void RelinkIdentifierMappings(ValueIdentifierMappingCollection<string> coll)
        {
            coll.Invoke(GlobalId == default, () => GlobalId = coll.GetGlobalId(this, nameof(GlobalId)));
            coll.Invoke(GlobalAlternateContactId == default && AlternateContactId != default, () => GlobalAlternateContactId = coll.GetGlobalId(this, nameof(GlobalAlternateContactId)));
            Address?.RelinkIdentifierMappings(coll);
        }

        #region AddressCdc

        /// <summary>
        /// Represents the CDC model for the related (child) database table '[Legacy].[Address]' (known uniquely as 'Address').
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public partial class AddressCdc : IPrimaryKey, IGlobalIdentifier<string>, ILinkIdentifierMapping<string>
        {
            /// <inheritdoc/>
            [JsonProperty("globalId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? GlobalId { get; set; }

            /// <summary>
            /// Gets or sets the Address Id '[Legacy].[Address].[AddressId]' column value. This column is used within the join.
            /// </summary>
            public int AddressId { get; set; }

            /// <summary>
            /// Gets or sets the Street1 '[Legacy].[Address].[Street1]' column value.
            /// </summary>
            [JsonProperty("street1", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Street1 { get; set; }

            /// <summary>
            /// Gets or sets the Street2 '[Legacy].[Address].[Street2]' column value.
            /// </summary>
            [JsonProperty("street2", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Street2 { get; set; }

            /// <summary>
            /// Gets or sets the Alternate Address Id '[Legacy].[Address].[AlternateAddressId]' column value.
            /// </summary>
            public int? AlternateAddressId { get; set; }

            /// <summary>
            /// Gets or sets the Global Alternate Address Id '[Legacy].[Address].[AlternateAddressId]' mapped identifier value.
            /// </summary>
            [JsonProperty("globalAlternateAddressId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? GlobalAlternateAddressId { get; set; }

            /// <inheritdoc/>
            public CompositeKey PrimaryKey => new CompositeKey(AddressId);

            /// <inheritdoc/>
            public CompositeKey TableKey => new CompositeKey(AddressId);

            /// <inheritdoc/>
            public async Task LinkIdentifierMappingsAsync(ValueIdentifierMappingCollection<string> coll, IIdentifierGenerator<string> idGen)
            {
                coll.AddAsync(GlobalId == default, async () => new ValueIdentifierMapping<string> { Value = this, Property = nameof(GlobalId), Schema = "Legacy", Table = "Address", Key = TableKey.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<AddressCdc>().ConfigureAwait(false) });
                coll.AddAsync(GlobalAlternateAddressId == default && AlternateAddressId != default, async () => new ValueIdentifierMapping<string> { Value = this, Property = nameof(GlobalAlternateAddressId), Schema = "Legacy", Table = "Address", Key = AlternateAddressId.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<AddressCdc>().ConfigureAwait(false) });
            }

            /// <inheritdoc/>
            public void RelinkIdentifierMappings(ValueIdentifierMappingCollection<string> coll)
            {
                coll.Invoke(GlobalId == default, () => GlobalId = coll.GetGlobalId(this, nameof(GlobalId)));
                coll.Invoke(GlobalAlternateAddressId == default && AlternateAddressId != default, () => GlobalAlternateAddressId = coll.GetGlobalId(this, nameof(GlobalAlternateAddressId)));
            }
        }

        /// <summary>
        /// Represents the CDC model collection for the related (child) database table '[Legacy].[Address]'.
        /// </summary>
        public partial class AddressCdcCollection : List<AddressCdc> { }

        #endregion
    }
}

#pragma warning restore
#nullable restore