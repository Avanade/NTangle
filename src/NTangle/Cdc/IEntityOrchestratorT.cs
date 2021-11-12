// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

namespace NTangle.Cdc
{
    /// <summary>
    /// Enables the core Change Data Capture (CDC) <see cref="IEntity"/> (aggregate root) orchestration capability.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="IEntity"/> <see cref="System.Type"/>.</typeparam>
    public interface IEntityOrchestrator<TEntity> : IEntityOrchestrator where TEntity : IEntity { }
}