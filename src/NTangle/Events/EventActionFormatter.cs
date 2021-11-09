// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using OnRamp.Utility;
using System;

namespace NTangle.Events
{
    /// <summary>
    /// Provides the <see cref="EventActionFormat"/> formatting capability. 
    /// </summary>
    public static class EventActionFormatter
    {
        /// <summary>
        /// Formats the <paramref name="action"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The <see cref="EventActionFormat"/>.</param>
        /// <returns>The formatted <see cref="EventData.Action"/>.</returns>
        public static string Format(string action, EventActionFormat? format = EventActionFormat.None) => format switch
        {
            EventActionFormat.PastTense => StringConverter.ToPastTense(action)!,
            _ => action
        };

        /// <summary>
        /// Formats the <paramref name="operationType"/> as the <see cref="EventData.Action"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <param name="format">The <see cref="EventActionFormat"/>.</param>
        /// <returns>The formatted <see cref="EventData.Action"/>.</returns>
        public static string Format(OperationType operationType, EventActionFormat? format = EventActionFormat.None) => Format(operationType.ToString(), format);
    }
}