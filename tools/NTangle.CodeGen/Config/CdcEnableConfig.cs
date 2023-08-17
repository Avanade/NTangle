// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents particpant tables for the <see cref="RootConfig.CdcEnable"/>.
    /// </summary>
    public class CdcEnableConfig
    {
        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        public string? Table { get; set; }
    }
}