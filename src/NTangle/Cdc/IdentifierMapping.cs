// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) identifier mapping model.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public class IdentifierMapping<T>
    {
        /// <summary>
        /// Gets or sets the table schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the key represented as string.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the global identifier (equivalent to <see cref="IGlobalIdentifier{T}.GlobalId"/>).
        /// </summary>
        public T? GlobalId { get; set; }
    }
}