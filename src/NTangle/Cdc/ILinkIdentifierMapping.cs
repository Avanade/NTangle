// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System.Threading.Tasks;

namespace NTangle.Cdc
{
    /// <summary>
    /// Provides the ability to link a value to a global identifier mapping that is provisioned at run-time.
    /// </summary>
    /// <typeparam name="T">The global identifier <see cref="System.Type"/>.</typeparam>
    public interface ILinkIdentifierMapping<T>
    {
        /// <summary>
        /// Link any new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="ValueIdentifierMappingCollection{T}"/>.</param>
        /// <param name="idgen">The <see cref="IIdentifierGenerator{T}"/>.</param>
        Task LinkIdentifierMappingsAsync(ValueIdentifierMappingCollection<T> coll, IIdentifierGenerator<T> idgen);

        /// <summary>
        /// Re-link the new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="ValueIdentifierMappingCollection{T}"/>.</param>
        void RelinkIdentifierMappings(ValueIdentifierMappingCollection<T> coll);
    }
}