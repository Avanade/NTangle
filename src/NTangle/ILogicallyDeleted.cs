// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle
{
    /// <summary>
    /// Enables an entity to identify whether it is logically deleted.
    /// </summary>
    public interface ILogicallyDeleted
    {
        /// <summary>
        /// Indicates whether the entity is considered logically deleted.
        /// </summary>
        bool? IsDeleted { get; set; }

        /// <summary>
        /// Clears all the non-primary key (i.e non <see cref="IPrimaryKey.PrimaryKey"/>) properties where <see cref="IsDeleted"/> (as the data is technically non-existing).
        /// </summary>
        void ClearWhereDeleted();
    }
}