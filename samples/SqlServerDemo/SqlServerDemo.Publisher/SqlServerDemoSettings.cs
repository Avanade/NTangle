namespace SqlServerDemo.Publisher;

/// <summary>
/// Provides the <b>SqlServerDemo</b> settings.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/>.</param>
public class SqlServerDemoSettings(IConfiguration configuration) : SettingsBase(configuration, "SqlServerDemo")
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