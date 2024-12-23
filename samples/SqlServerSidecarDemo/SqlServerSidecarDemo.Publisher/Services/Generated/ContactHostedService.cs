/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace SqlServerSidecarDemo.Publisher.Services;

/// <summary>
/// Provides the Change Data Capture (CDC) <see cref="ContactCdc"/> entity (aggregate root) <see cref="HostedService{T}"/> capabilities (database table '[Legacy].[Contact]').
/// </summary>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
/// <param name="logger">The <see cref="ILogger"/>.</param>
/// <param name="settings">The <see cref="SettingsBase"/>.</param>
/// <param name="synchronizer"> The <see cref="IServiceSynchronizer"/>.</param>
public partial class ContactHostedService(IServiceProvider serviceProvider, ILogger<ContactHostedService> logger, SettingsBase settings, IServiceSynchronizer synchronizer) 
    : CdcHostedService<IContactOrchestrator, ContactCdc>(serviceProvider, logger, settings, synchronizer) { }