// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Mapping;
using NTangle.Cdc;

namespace NTangle.Data
{
    /// <summary>
    /// Represents the <see cref="VersionTracker"/> database mapper. 
    /// </summary>
    public class VersionTrackingMapper : DatabaseMapperEx<VersionTracker>
    {
        /// <inheritdoc/>
        protected override void OnMapFromDb(DatabaseRecord record, VersionTracker value, OperationTypes operationType)
        {
            value.Key = record.GetValue<string>(nameof(VersionTracker.Key));
            value.Hash = record.GetValue<string>(nameof(VersionTracker.Hash));
        }

        /// <inheritdoc/>
        protected override void OnMapToDb(VersionTracker value, DatabaseParameterCollection parameters, OperationTypes operationType)
        {
            parameters.AddParameter(nameof(VersionTracker.Key), value.Key);
            parameters.AddParameter(nameof(VersionTracker.Hash), value.Hash);
        }
    }
}