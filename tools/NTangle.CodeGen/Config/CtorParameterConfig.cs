// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp.Config;
using OnRamp.Utility;
using System.Threading.Tasks;

namespace NTangle.CodeGen.Config
{
    /// <summary>
    /// Represents the <see cref="TableConfig"/> related .NET constructor parameter configuration.
    /// </summary>
    /// <remarks>This is set up internally at runtime versus externally configured.</remarks>
    public class CtorParameterConfig : ConfigBase<RootConfig, TableConfig>
    {
        /// <summary>
        /// Gets or sets the unique parameter name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the .NET <see cref="Type"/>.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        public string? PrivateName { get; set; }

        /// <summary>
        /// Gets or sets the overriding argument name.
        /// </summary>
        public string? ArgumentName { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            PrivateName = DefaultWhereNull(PrivateName, () => StringConverter.ToPrivateCase(Name));
            ArgumentName = DefaultWhereNull(ArgumentName, () => StringConverter.ToCamelCase(Name));
            return Task.CompletedTask;
        }
    }
}