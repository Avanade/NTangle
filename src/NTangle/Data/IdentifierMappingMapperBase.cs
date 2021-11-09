// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using NTangle.Cdc;
using System;
using System.Collections.Generic;
using System.Data;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="IdentifierMapping{T}"/> database mapper. 
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public abstract class IdentifierMappingMapperBase<T> : IDatabaseMapper<IdentifierMapping<T>>, IIdentifierMappingTvp<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierMappingMapperBase{T}"/> class.
        /// </summary>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public IdentifierMappingMapperBase(string dbTypeName) => DbTypeName = dbTypeName ?? throw new ArgumentNullException(nameof(dbTypeName));

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <inheritdoc/>
        public IdentifierMapping<T> MapFromDb(DatabaseRecord record) => new()
        {
            Schema = record.GetValue<string>(nameof(IdentifierMapping<T>.Schema)),
            Table = record.GetValue<string>(nameof(IdentifierMapping<T>.Table)),
            Key = record.GetValue<string>(nameof(IdentifierMapping<T>.Key)),
            GlobalId = record.GetValue<T>(nameof(IdentifierMapping<T>.GlobalId))
        };

        /// <inheritdoc/>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<IdentifierMapping<T>> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(IdentifierMapping<T>.Schema), typeof(string));
            dt.Columns.Add(nameof(IdentifierMapping<T>.Table), typeof(string));
            dt.Columns.Add(nameof(IdentifierMapping<T>.Key), typeof(string));
            dt.Columns.Add(nameof(IdentifierMapping<T>.GlobalId), typeof(T));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            foreach (var item in list)
            {
                tvp.AddRow(item.Schema, item.Table, item.Key, item.GlobalId);
            }

            return tvp;
        }
    }
}