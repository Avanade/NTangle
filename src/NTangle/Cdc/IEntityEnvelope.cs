// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides the required CDC envelope properties for an <see cref="IEntity"/>.
    /// </summary>
    public interface IEntityEnvelope : IEntity
    {
        /// <summary>
        /// Gets or sets the database CDC <see cref="CdcOperationType"/>.
        /// </summary>
        CdcOperationType DatabaseOperationType { get; set; }

        /// <summary>
        /// Gets or sets the database tracking hash code.
        /// </summary>
        string? DatabaseTrackingHash { get; set; }

        /// <summary>
        /// Gets or sets the database log sequence number (LSN).
        /// </summary>
        byte[] DatabaseLsn { get; set; }
    }
}