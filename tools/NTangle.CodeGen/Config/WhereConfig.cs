// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Newtonsoft.Json;
using OnRamp;
using OnRamp.Config;
using System.Linq;
using System.Threading.Tasks;

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents the table where configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Where", Title = "'Where' object (database-driven)",
        Description = "This should only be used where the column value is largely immutable; otherwise, unintended side-effects may occur. _NTangle_ uses the condition explictily and does not attempt to handle value change to infer creation " +
        "or deletion of data as a result of the underlying change; as such, this should be used cautiously. Note that the `where` is applied when querying the `cdc.fn_cdc_get_all_changes_...` function, not the underlying table itself.")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    public class WhereConfig : ConfigBase<RootConfig, TableConfig>
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The column name.", IsMandatory = true, IsImportant = true,
            Description = "The column name.")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the where nullability or `Value` clause operator.
        /// </summary>
        [JsonProperty("nullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The where nullability clause operator", IsImportant = true, Options = new string[] { "ISNULL", "ISNOTNULL" },
            Description = "This enables statements such as `WHERE (COL IS NULL)` or `WHERE (COL IS NULL OR COL = VALUE)` (where .")]
        public string? Nullable { get; set; }

        /// <summary>
        /// Gets or sets the where clause equality operator.
        /// </summary>
        [JsonProperty("operator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The where clause equality operator", IsImportant = true, Options = new string[] { "EQ", "NE", "LT", "LE", "GT", "GE", "LIKE" },
            Description = "Defaults to `EQ` where `Value` is specified.")]
        public string? Operator { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [CodeGenProperty("Key", Title = "The comparison value", Description = "This must be valid formatted/escaped SQL.")]
        public string? Value { get; set; }

        /// <summary>
        /// Gets the SQL <see cref="Nullable"/> operator.
        /// </summary>
        public string SqlNullOperator => Nullable == "ISNULL" ? "IS NULL" : "IS NOT NULL";

        /// <summary>
        /// Gets the SQL <see cref="Operator"/>.
        /// </summary>
        public string SqlOperator => Operator switch
        {
            "NE" => "<>",
            "LT" => "<",
            "LE" => "<=",
            "GT" => ">",
            "GE" => ">=",
            "LIKE" => "LIKE",
            _ => "="
        };

        /// <inheritdoc/>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Where", Name);

        /// <inheritdoc/>
        protected override Task PrepareAsync()
        {
            if (Operator == null && Value != Nullable)
                Operator = "EQ";

            var c = Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault() ?? throw new CodeGenException(this, nameof(Name), $"Column '{Name}' (table '[{Parent!.Schema}].[{Parent!.Name}]') not found in database.");
            if (Nullable == null && Value == null)
                throw new CodeGenException(this, nameof(Value), $"The {nameof(Nullable)} and/or {nameof(Value)} properties are required for a Where clause.");

            return Task.CompletedTask;
        }
    }
}