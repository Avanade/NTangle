// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Mapping;
using NTangle.Cdc;
using System;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="IdentifierMapping{T}"/> database mapper. 
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public class IdentifierMappingMapper<T> : IDatabaseMapper<IdentifierMapping<T>>
    {
        /// <inheritdoc/>
        public IdentifierMapping<T>? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            Schema = record.GetValue<string>(nameof(IdentifierMapping<T>.Schema)),
            Table = record.GetValue<string>(nameof(IdentifierMapping<T>.Table)),
            Key = record.GetValue<string>(nameof(IdentifierMapping<T>.Key)),
            GlobalId = record.GetValue<T>(nameof(IdentifierMapping<T>.GlobalId))
        };

        /// <inheritdoc/>
        void IDatabaseMapper<IdentifierMapping<T>>.MapToDb(IdentifierMapping<T>? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}