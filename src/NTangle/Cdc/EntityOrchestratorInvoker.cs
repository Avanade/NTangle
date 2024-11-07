// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using CoreEx;
using CoreEx.Invokers;

namespace NTangle.Cdc
{
    /// <summary>
    /// Represents the <see cref="IEntityOrchestrator"/> invoker.
    /// </summary>
    public class EntityOrchestratorInvoker : InvokerBase<IEntityOrchestrator>
    {
        private static EntityOrchestratorInvoker? _default;

        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static EntityOrchestratorInvoker Current => ExecutionContext.GetService<EntityOrchestratorInvoker>() ?? (_default ??= new EntityOrchestratorInvoker());

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOrchestratorInvoker"/> class.
        /// </summary>
        public EntityOrchestratorInvoker() => CallerLoggerFormatter = args => $"{InvokeArgs.DefaultCallerLogFormatter(args)}<ExecutionId:{((IEntityOrchestrator)args.Owner!).ExecutionId}>";
    }
}