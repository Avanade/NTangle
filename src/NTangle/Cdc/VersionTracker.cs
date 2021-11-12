// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) version (see <see cref="Hash"/>) tracking model.
    /// </summary>
    public class VersionTracker
    {
        /// <summary>
        /// Gets or sets the key represented as string.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the hash code represented as a string.
        /// </summary>
        public string? Hash { get; set; }
    }
}