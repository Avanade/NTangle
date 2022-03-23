// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx.Entities;

namespace NTangle
{
    /// <summary>
    /// Defines the entity (aggregate root).
    /// </summary>
    public interface IEntity : IPrimaryKey, IETag { }
}