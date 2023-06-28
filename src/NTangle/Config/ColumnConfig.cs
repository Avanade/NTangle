// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Config
{
    /// <summary>
    /// Represents the <see cref="TableConfig"/> related column configuration.
    /// </summary>
    /// <remarks>This is set up internally at runtime versus externally configured.</remarks>
    public sealed class ColumnConfig : ColumnConfigBase<TableConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnConfig"/> class.
        /// </summary>
        internal ColumnConfig() { }

        /// <summary>
        /// Indicates whether the column has been marked for exclusion.
        /// </summary>
        public bool IsExcluded { get; set; }
    }
}