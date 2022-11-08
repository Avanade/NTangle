// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx.DbSchema;

namespace NTangle.Config
{
    /// <summary>
    /// Defines the base configuration for a column within a database table.
    /// </summary>
    public interface IColumnConfig
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="DbColumnSchema"/> configuration.
        /// </summary>
        DbColumnSchema? DbColumn { get; set; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        string QualifiedName { get; }

        /// <summary>
        /// Indicates whether the column should be included on a delete.
        /// </summary>
        bool IncludeColumnOnDelete { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        string DotNetType { get; }

        /// <summary>
        /// Indicates whether the .NET property is nullable.
        /// </summary>
        bool IsDotNetNullable { get; }

        /// <summary>
        /// Gets or sets the name alias.
        /// </summary>
        string? NameAlias { get; set; }

        /// <summary>
        /// Indicates whether the column should not be serialized when creating an .NET entity equivalent.
        /// </summary>
        bool IgnoreSerialization { get; set; }

        /// <summary>
        /// Indicates whether the column is the "IsDeleted" column.
        /// </summary>
        bool IsIsDeletedColumn { get; }

        /// <summary>
        /// Gets or sets the identifier mapping schema name.
        /// </summary>
        string? IdentifierMappingSchema { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping table name.
        /// </summary>
        string? IdentifierMappingTable { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping alias.
        /// </summary>
        string? IdentifierMappingAlias { get; set; }

        /// <summary>
        /// Gets or sets the identifier mapping parent column configuration.
        /// </summary>
        IColumnConfig? IdentifierMappingParent { get; set; }
    }
}