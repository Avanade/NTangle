// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx;
using NTangle.Cdc;
using System;

namespace NTangle.Data
{
    /// <summary>
    /// Provides the <see cref="BatchTracker"/> <see cref="IDatabaseMapper{T}"/> functionality.
    /// </summary>
    public class BatchTrackerMapper : IDatabaseMapper<BatchTracker>
    {
        /// <inheritdoc/>
        public BatchTracker MapFromDb(DatabaseRecord r) => new()
        {
            Id = r.GetValue<long>("BatchTrackingId"),
            CreatedDate = r.GetValue<DateTime>("CreatedDate"),
            IsComplete = r.GetValue<bool>("IsComplete"),
            CompletedDate = r.GetValue<DateTime?>("CompletedDate"),
            CorrelationId = r.GetValue<string?>("CorrelationId"),
            HasDataLoss = r.GetValue<bool>("HasDataLoss")
        };
    }
}