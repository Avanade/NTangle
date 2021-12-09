// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Console
{
    /// <summary>
    /// Defines the Microsoft SQL Server deployment management options.
    /// </summary>
    public enum SqlServerDeployment
    {
        /// <summary>
        /// Use <b>DbEx</b> (<see href="https://github.com/Avanade/dbex"/>) to manage the database deployment.
        /// </summary>
        DbEx,

        /// <summary>
        /// Use <b>DACPAC</b> <see href="https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications"/> to manage the database deployment.
        /// </summary>
        Dacpac
    }
}