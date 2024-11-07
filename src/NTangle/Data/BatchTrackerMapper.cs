// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database;
using CoreEx.Database.Mapping;
using CoreEx.Mapping;
using NTangle.Cdc;
using System;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the <see cref="BatchTracker"/> <see cref="IDatabaseMapper{T}"/> functionality.
    /// </summary>
    public class BatchTrackerMapper : DatabaseMapperEx<BatchTracker, BatchTrackerMapper>
    {
        /// <inheritdoc/>
        protected override void OnMapFromDb(DatabaseRecord record, BatchTracker value, OperationTypes operationType)
        {
            value.Id = record.GetValue<long>("BatchTrackingId");
            value.CreatedDate = record.GetValue<DateTime>(nameof(BatchTracker.CreatedDate));
            value.IsComplete = record.GetValue<bool>(nameof(BatchTracker.IsComplete));
            value.CompletedDate = record.GetValue<DateTime?>(nameof(BatchTracker.CompletedDate));
            value.CorrelationId = record.GetValue<string?>(nameof(BatchTracker.CorrelationId));
            value.HasDataLoss = record.GetValue<bool>(nameof(BatchTracker.HasDataLoss));
        }

        /// <inheritdoc/>
        /// <remarks>The <see cref="BatchTracker.CreatedDate"/>, <see cref="BatchTracker.CompletedDate"/>, <see cref="BatchTracker.IsComplete"/> and <see cref="BatchTracker.CorrelationId"/> are updated by the underlying data source.</remarks>
        protected override void OnMapToDb(BatchTracker value, DatabaseParameterCollection parameters, OperationTypes operationType)
        {
            parameters.AddParameter("BatchTrackingId", value.Id);
            parameters.AddParameter(nameof(BatchTracker.HasDataLoss), value.HasDataLoss);
        }
    }
}