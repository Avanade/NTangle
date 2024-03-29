﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Database.SqlServer;
using NTangle.Cdc;
using System.Collections.Generic;

namespace NTangle.Data
{
    /// <summary>
    /// Enables the <see cref="CreateTableValuedParameter"/> capability.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public interface IIdentifierMappingTvp<T>
    {
        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The <see cref="IdentifierMapping{T}"/> list.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        TableValuedParameter CreateTableValuedParameter(IEnumerable<IdentifierMapping<T>> list);
    }
}