// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents a <see cref="ValueIdentifierMapping{T}"/> collection.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public class ValueIdentifierMappingCollection<T> : List<ValueIdentifierMapping<T>>
    {
        /// <summary>
        /// Adds the <see cref="ValueIdentifierMapping{T}"/> created by the <paramref name="link"/> function only where the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition value.</param>
        /// <param name="link">The function to the create the linked <see cref="ValueIdentifierMapping{T}"/>.</param>
        public async Task AddAsync(bool condition, Func<Task<ValueIdentifierMapping<T>>> link)
        {
            if (condition && link != null)
            {
                var item = await link().ConfigureAwait(false);
                Add(item);
            }
        }

        /// <summary>
        /// Gets the <see cref="IdentifierMapping{T}.GlobalId"/> for the specified <paramref name="value"/> and <paramref name="property"/> name.
        /// </summary>
        /// <param name="value">The related (owning) value.</param>
        /// <param name="property">The property name.</param>
        /// <returns>The <see cref="IdentifierMapping{T}.GlobalId"/>.</returns>
        public T GetGlobalId(object value, string property) => this.Single(x => x.Value == value && x.Property == property).GlobalId ?? default!;

        /// <summary>
        /// Invokes the <paramref name="link"/> action where the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition value.</param>
        /// <param name="link">The action to invoke.</param>
        /// <remarks>Note that this method does not affect the underlying collection and is only enabled to simplify the coding (code-generation) of related conditional invoking.</remarks>
        public void Invoke(bool condition, Action link)
        {
            if (condition && link != null)
                link();
        }
    }
}