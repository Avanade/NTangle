// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;
using System;

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) batch tracker.
    /// </summary>
    public class BatchTracker : IIdentifier<long>
    {
        /// <summary>
        /// Gets or sets the batch identifer.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the batch created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Indicates whether the batch has been marked as completed.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the completed date.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Indicates whether there is/was possible data loss; that the CDC data has possibly been cleaned up before being successfully processed.
        /// </summary>
        public bool HasDataLoss { get; set; }
    }
}