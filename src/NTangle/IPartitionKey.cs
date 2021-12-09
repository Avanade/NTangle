﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle
{
    /// <summary>
    /// Provides the <see cref="PartitionKey"/>.
    /// </summary>
    public interface IPartitionKey
    {
        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public string? PartitionKey { get; }
    }
}