// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Data
{
    /// <summary>
    /// Provides the <see cref="DatabaseInfo"/>.
    /// </summary>
    public interface IDatabaseInfo
    {
        /// <summary>
        /// Gets the <see cref="DatabaseInfo"/>.
        /// </summary>
        static abstract DatabaseInfo DatabaseInfo { get; }
    }
}