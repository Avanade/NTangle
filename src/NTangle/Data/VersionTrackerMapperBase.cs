// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Database;
using CoreEx.Database.SqlServer;
using CoreEx.Mapping;
using NTangle.Cdc;
using System;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="VersionTracker"/> database mapper. 
    /// </summary>
    /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
    public abstract class VersionTrackingMapperBase(string dbTypeName) : IDatabaseMapper<VersionTracker>
    {
        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; } = dbTypeName.ThrowIfNull(nameof(dbTypeName));

        /// <inheritdoc/>
        public VersionTracker? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            Key = record.GetValue<string>(nameof(VersionTracker.Key)),
            Hash = record.GetValue<string>(nameof(VersionTracker.Hash))
        };

        /// <inheritdoc/>
        void IDatabaseMapper<VersionTracker>.MapToDb(VersionTracker? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}