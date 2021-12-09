// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using DbEx.SqlServer;
using System;
using System.Collections.Generic;

namespace NTangle.Data
{
    /// <summary>
    /// Defines the <see cref="CreateTableValuedParameter"/> capability.
    /// </summary>
    /// <typeparam name="T">The underlying list <see cref="Type"/>.</typeparam>
    public interface IDatabaseTvp<T>
    {
        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list of <see cref="Type"/> <typeparamref name="T"/> to include as the <see cref="TableValuedParameter.Value"/>.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        TableValuedParameter CreateTableValuedParameter(IEnumerable<T> list);
    }
}