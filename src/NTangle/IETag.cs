﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle
{
    /// <summary>
    /// Provides the <see cref="ETag"/> property for the likes of versioning.
    /// </summary>
    public interface IETag
    {
        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        string? ETag { get; set; }
    }
}