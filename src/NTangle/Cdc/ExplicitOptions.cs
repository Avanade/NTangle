// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides options to control behavior where performing an explicit execution.
    /// </summary>
    public class ExplicitOptions
    {
        /// <summary>
        /// Indicates whether to assume a <see cref="CdcOperationType.Delete"/> where not found; otherwise, ignore (i.e. do not process further).
        /// </summary>
        /// <remarks>This is only applicable for the root keys as any child keys where not found will be ignored as there is no means to to walk back up the join hierarchy.
        /// <para>Defaults to <c>true</c>.</para></remarks>
        public bool AssumeDeleteWhereNotFound { get; set; } = true;

        /// <summary>
        /// Indicates whether to assume a <see cref="CdcOperationType.Create"/> where no existing <see cref="VersionTracker"/>; otherwise, <see cref="CdcOperationType.Update"/>.
        /// </summary>
        /// <remarks>Defaults to <c>true</c>.</remarks>
        public bool AssumeCreateWhereNoVersion { get; set; } = true;

        /// <summary>
        /// Indicates whether to always publish events even where no changes (<see cref="VersionTracker"/>) have been detected.
        /// </summary>
        /// <remarks>Defaults to <c>false</c>.</remarks>
        public bool AlwaysPublishEvents { get; set; } = false;
    }
}