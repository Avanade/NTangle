// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Mapping;
using NTangle.Cdc;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="IdentifierMapping{T}"/> database mapper. 
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public class IdentifierMappingMapper<T> : DatabaseMapperEx<IdentifierMapping<T>>
    {
        /// <inheritdoc/>
        protected override void OnMapFromDb(DatabaseRecord record, IdentifierMapping<T> value, OperationTypes operationType)
        {
            value.Schema = record.GetValue<string>(nameof(IdentifierMapping<T>.Schema));
            value.Table = record.GetValue<string>(nameof(IdentifierMapping<T>.Table));
            value.Key = record.GetValue<string>(nameof(IdentifierMapping<T>.Key));
            value.GlobalId = record.GetValue<T>(nameof(IdentifierMapping<T>.GlobalId));
        }

        /// <inheritdoc/>
        protected override void OnMapToDb(IdentifierMapping<T> value, DatabaseParameterCollection parameters, OperationTypes operationType)
        {
            parameters.AddParameter(nameof(IdentifierMapping<T>.Schema), value.Schema);
            parameters.AddParameter(nameof(IdentifierMapping<T>.Table), value.Table);
            parameters.AddParameter(nameof(IdentifierMapping<T>.Key), value.Key);
            parameters.AddParameter(nameof(IdentifierMapping<T>.GlobalId), value.GlobalId);
        }
    }
}