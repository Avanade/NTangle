// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Mapping;
using NTangle.Cdc;
using System;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="VersionTracker"/> database mapper. 
    /// </summary>
    public class VersionTrackingMapper : IDatabaseMapper<VersionTracker>
    {
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