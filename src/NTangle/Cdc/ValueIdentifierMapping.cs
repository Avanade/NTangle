// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System.Text.Json.Serialization;

namespace NTangle.Cdc
{
    /// <summary>
    /// Used internally within the <see cref="EntityOrchestratorResult"/> to link the <see cref="IdentifierMapping{T}.GlobalId"/> to the related <see cref="Value"/> for a specified <see cref="Property"/>.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public class ValueIdentifierMapping<T> : IdentifierMapping<T>
    {
        /// <summary>
        /// Gets or sets the corresponding property name.
        /// </summary>
        [JsonIgnore]
        public string? Property { get; set; }

        /// <summary>
        /// Gets or sets the related (owning) value.
        /// </summary>
        [JsonIgnore]
        public object? Value { get; set; }
    }
}