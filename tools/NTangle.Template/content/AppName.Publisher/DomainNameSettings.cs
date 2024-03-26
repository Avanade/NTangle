namespace AppName.Publisher;

/// <summary>
/// Provides the <b>AppName</b> settings.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/>.</param>
public class DomainNameSettings(IConfiguration configuration) : SettingsBase(configuration, "DomainName")
{
    /// <summary>
    /// Gets the SQL Server database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__SqlDb");

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
}