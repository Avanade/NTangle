// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Configuration;
using System;

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides standardized access to the <see cref="SettingsBase"/> settings.
    /// </summary>
    public static class CdcSettings
    {
        /// <summary>
        /// The root settings section name.
        /// </summary>
        public const string RootSettingsSectionName = "NTangle";

        /// <summary>
        /// The default settings section name.
        /// </summary>
        public const string DefaultsSettingsSectionName = "Default";

        /// <summary>
        /// The enabled settings name.
        /// </summary>
        public const string EnabledSettingsName = "Enabled";

        /// <summary>
        /// Gets the CDC <paramref name="settings"/> value for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The settings value <see cref="Type"/>.</typeparam>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="section">The settings section name.</param>
        /// <param name="name">The individual settings name.</param>
        /// <returns>The The settings value.</returns>
        /// <remarks>Will probe for the <paramref name="section"/> name first, then the <see cref="DefaultsSettingsSectionName"/>.</remarks>
        public static T GetCdcValue<T>(this SettingsBase settings, string section, string name)
            => settings.GetValue<T>($"{RootSettingsSectionName}__{section}__{name}") ?? settings.GetValue<T>($"{RootSettingsSectionName}__{DefaultsSettingsSectionName}__{name}");
    }
}