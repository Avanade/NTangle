// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Database.SqlServer;
using CoreEx.Mapping;
using NTangle.Cdc;
using System;
using System.Collections.Generic;
using System.Data;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="VersionTracker"/> database mapper. 
    /// </summary>
    public abstract class VersionTrackingMapperBase : IDatabaseMapper<VersionTracker>, IDatabaseTvp<VersionTracker>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionTrackingMapperBase"/> class.
        /// </summary>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public VersionTrackingMapperBase(string dbTypeName) => DbTypeName = dbTypeName ?? throw new ArgumentNullException(nameof(dbTypeName));

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <inheritdoc/>
        public VersionTracker? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            Key = record.GetValue<string>(nameof(VersionTracker.Key)),
            Hash = record.GetValue<string>(nameof(VersionTracker.Hash))
        };

        /// <inheritdoc/>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<VersionTracker> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(VersionTracker.Key), typeof(string));
            dt.Columns.Add(nameof(VersionTracker.Hash), typeof(string));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            foreach (var item in list)
            {
                tvp.AddRow(item.Key, item.Hash);
            }

            return tvp;
        }

        /// <inheritdoc/>
        void IDatabaseMapper<VersionTracker>.MapToDb(VersionTracker? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}