// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the possible CDC operations types.
    /// </summary>
    /// <remarks>The values are as per the SQL Server CDC '<c>__$operation</c>', see <see href="https://docs.microsoft.com/en-us/sql/relational-databases/system-functions/cdc-fn-cdc-get-all-changes-capture-instance-transact-sql"/>.</remarks>
    public enum CdcOperationType
    {
        /// <summary>
        /// An unknown/unspecified operation type.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// A <b>create</b> operation.
        /// </summary>
        Create = 2,

        /// <summary>
        /// An <b>update</b> operation.
        /// </summary>
        Update = 4,

        /// <summary>
        /// A <b>delete</b> operation.
        /// </summary>
        Delete = 1
    }
}