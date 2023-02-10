// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp.Config;
using DbEx.DbSchema;
using System;
using System.Threading.Tasks;

namespace NTangle.Config
{
    /// <summary>
    /// Represents the base configuration for a column within a database table.
    /// </summary>
    /// <typeparam name="TParent">The parent <see cref="Type"/>.</typeparam>
    public abstract class ColumnConfigBase<TParent> : ConfigBase<RootConfig, TParent>, IColumnConfig where TParent : ConfigBase, ITableReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnConfig"/> class.
        /// </summary>
        internal ColumnConfigBase() { }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="DbColumnSchema"/> configuration.
        /// </summary>
        public DbColumnSchema? DbColumn { get; set; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        public string QualifiedName => $"[{Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Indicates whether the column should be included on a delete.
        /// </summary>
        public bool IncludeColumnOnDelete { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType => DbColumn!.DotNetType;

        /// <summary>
        /// Indicates whether the .NET property is nullable.
        /// </summary>
        public bool IsDotNetNullable => DbColumn!.IsNullable || DotNetType == "string" || DotNetType == "byte[]";

        /// <summary>
        /// Gets or sets the name alias.
        /// </summary>
        public string? NameAlias { get; set; }

        /// <summary>
        /// Indicates whether the column should not be serialized when creating a .NET entity equivalent.
        /// </summary>
        public bool IgnoreSerialization { get; set; }

        /// <summary>
        /// Indicates whether the column is the "IsDeleted" column.
        /// </summary>
        public bool IsIsDeletedColumn { get; }

        /// <summary>
        /// Gets or sets the identifier mapping schema name.
        /// </summary>
        public string? IdentifierMappingSchema { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping table name.
        /// </summary>
        public string? IdentifierMappingTable { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping alias.
        /// </summary>
        public string? IdentifierMappingAlias { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping parent column configuration.
        /// </summary>
        public IColumnConfig? IdentifierMappingParent { get; set; }

        /// <summary>
        /// Indicates whether the column is being used within a <see cref="JoinOnConfig"/>.
        /// </summary>
        public bool IsUsedInJoinOn { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            NameAlias = DefaultWhereNull(NameAlias, () => Root!.RenameForDotNet(Name));
            return Task.CompletedTask;
        }
    }
}