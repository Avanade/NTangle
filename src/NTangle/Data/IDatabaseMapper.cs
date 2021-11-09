// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

namespace NTangle.Data
{
    /// <summary>
    /// Defines a database mapper.
    /// </summary>
    /// <typeparam name="T">The resulting <see cref="Type"/>.</typeparam>
    public interface IDatabaseMapper<T>
    {
        /// <summary>
        /// Maps from a <see cref="DatabaseRecord"/> creating a corresponding instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="record">The <see cref="DatabaseRecord"/>.</param>
        /// <returns>The corresponding instance of <typeparamref name="T"/>.</returns>
        T MapFromDb(DatabaseRecord record);
    }
}