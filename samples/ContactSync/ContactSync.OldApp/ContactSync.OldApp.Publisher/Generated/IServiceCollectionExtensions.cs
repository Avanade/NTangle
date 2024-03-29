/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the generated <see cref="NTangle.Cdc.IEntityOrchestrator"/> scoped services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddGeneratedCdcOrchestratorServices(this IServiceCollection services)
        => services.AddScoped<IContactOrchestrator, ContactOrchestrator>();

    /// <summary>
    /// Adds the generated <see cref="NTangle.Services.CdcService{TOrchestrator, TEntity}"/> scoped services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddGeneratedCdcServices(this IServiceCollection services)
        => services.AddScoped<ContactService>();
}