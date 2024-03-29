﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx.DbSchema;
using OnRamp;
using OnRamp.Config;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents the table join on condition configuration.
    /// </summary>
    [CodeGenClass("JoinOn", Title = "'JoinOn' object (database-driven)",
        Description = "The `JoinOn` object defines the join on characteristics for a `Join` object.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml

```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    public sealed class JoinOnConfig : ConfigBase<RootConfig, JoinConfig>
    {
        /// <inheritdoc/>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("JoinOn", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the join column (from the `Join` table).
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The name of the join column (from the `Join` table).", IsMandatory = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the join to column.
        /// </summary>
        [JsonPropertyName("toColumn")]
        [CodeGenProperty("Key", Title = "The name of the join to column.", IsImportant = true,
            Description = "Defaults to `Name`; i.e. assumes same name.")]
        public string? ToColumn { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement for the join on bypassing the corresponding `ToColumn` specification.
        /// </summary>
        [JsonPropertyName("toStatement")]
        [CodeGenProperty("Key", Title = "The SQL statement for the join on bypassing the corresponding `ToColumn` specification.")]
        public string? ToStatement { get; set; }

        #endregion

        #region Non-Config

        /// <summary>
        /// Gets or sets the <see cref="Name"/> alias.
        /// </summary>
        public string? NameAlias { get; set; }

        /// <summary>
        /// Gets the <see cref="ToColumn"/> <see cref="DbColumnSchema"/>.
        /// </summary>
        public DbColumnSchema? ToDbColumn { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ToColumn"/> alias.
        /// </summary>
        public string? ToColumnAlias { get; set; }

        #endregion

        /// <inheritdoc/>
        protected override Task PrepareAsync()
        {
            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            var c = Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault() ?? throw new CodeGenException(this, nameof(Name), $"Column '{Name}' (table '[{Parent!.Schema}].[{Parent!.Name}]') not found in database.");
            var cc = Parent!.Columns.Where(x => x.Name == Name).Single();
            cc.IsUsedInJoinOn = true;
            NameAlias = cc.NameAlias ?? Name;

            if (string.IsNullOrEmpty(ToStatement))
            {
                ToColumn = DefaultWhereNull(ToColumn, () => Name);
                ToDbColumn = Root!.DbTables!.Where(x => x.Schema == Parent.JoinToSchema && x.Name == Parent.JoinToTable).SingleOrDefault()?.Columns.Where(x => x.Name == ToColumn).SingleOrDefault();
                if (ToDbColumn == null)
                    throw new CodeGenException(this, nameof(ToColumn), $"ToColumn '{ToColumn}' (table '[{Parent.JoinToSchema}].[{Parent.JoinToTable}]') not found in database.");

                if (Parent.JoinToSchema == Parent!.Parent!.Schema && Parent.JoinToTable == Parent!.Parent!.Table)
                {
                    if (Parent!.Parent!.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (table '[{Parent.JoinToSchema}].[{Parent.JoinToTable}]') not found in Table/Join configuration.");

                    var jtc = Parent!.Parent!.Columns.SingleOrDefault(x => x.Name == ToColumn);
                    ToColumnAlias = jtc?.NameAlias ?? Name;
                    if (jtc != null)
                        jtc.IsUsedInJoinOn = true;
                }
                else
                {
                    var t = Parent!.Parent!.Joins!.Where(x => Parent.JoinToSchema == x.Schema && Parent.JoinToTable == x.Table).SingleOrDefault();
                    if (t == null || t.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (table '[{Parent.JoinToSchema}].[{Parent.JoinToTable}]') not found in Table/Join configuration.");

                    var jtc = t.Columns.SingleOrDefault(x => x.Name == ToColumn);
                    ToColumnAlias = jtc?.NameAlias ?? Name;
                    if (jtc != null)
                        jtc.IsUsedInJoinOn = true;
                }
            }

            return Task.CompletedTask;
        }
    }
}