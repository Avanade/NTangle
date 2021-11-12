// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Events
{
    /// <summary>
    /// Defines the <see cref="EventData.Action"/> format.
    /// </summary>
    public enum EventActionFormat
    {
        /// <summary>
        /// No formatting, leave action as-is.
        /// </summary>
        None,

        /// <summary>
        /// The action as past-tense.
        /// </summary>
        PastTense
    }
}