namespace AppName.Publisher;

/// <summary>
/// Provides the <b>AppName</b> settings.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/>.</param>
public class DomainNameSettings(IConfiguration configuration) : SettingsBase(configuration, "DomainName")
{
    /// <summary>
    /// Gets the "source" SQL Server database connection string.
    /// </summary>
    public string SourceDbConnectionString => GetRequiredValue<string>("ConnectionStrings__SourceDb");

    /// <summary>
    /// Gets the "sidecar" SQL Server database connection string.
    /// </summary>
    public string SidecarDbConnectionString => GetRequiredValue<string>("ConnectionStrings__SidecarDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}