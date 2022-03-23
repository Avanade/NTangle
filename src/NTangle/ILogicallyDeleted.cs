// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;

namespace NTangle
{
    /// <summary>
    /// Enables an entity to identify whether it is logically deleted.
    /// </summary>
    public interface ILogicallyDeletedExtended : ILogicallyDeleted
    {
        /// <summary>
        /// Clears all the non-primary key (i.e non <see cref="IPrimaryKey.PrimaryKey"/>) properties where <see cref="ILogicallyDeleted.IsDeleted"/> (as the data is technically non-existing).
        /// </summary>
        void ClearWhereDeleted();
    }
}